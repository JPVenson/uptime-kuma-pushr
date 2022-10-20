using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UptimeKuma.Pushr.Services.HostedServices.StatusClient;
using UptimeKuma.Pushr.Services.HostedServices.TaskRunner.Options;
using UptimeKuma.Pushr.Services.MonitorStore;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.Util;

namespace UptimeKuma.Pushr.Services.HostedServices.TaskRunner;

public class TaskRunnerService : BackgroundService
{
	private readonly ITaskStoreService _taskStoreService;
	private readonly IHostApplicationLifetime _hostApplicationLifetime;
	private readonly IOptions<TaskRunnerOptions> _taskRunnerOptions;
	private readonly IPushQueueService _pushQueueService;
	private readonly IMonitorStoreService _monitorStoreService;
	private readonly ITaskRunnerNotifyService _runnerNotifyService;

	public TaskRunnerService(ITaskStoreService taskStoreService, 
		IHostApplicationLifetime hostApplicationLifetime,
		IOptions<TaskRunnerOptions> taskRunnerOptions,
		IPushQueueService pushQueueService,
		IMonitorStoreService monitorStoreService,
		ITaskRunnerNotifyService runnerNotifyService)
	{
		_taskStoreService = taskStoreService;
		_hostApplicationLifetime = hostApplicationLifetime;
		_taskRunnerOptions = taskRunnerOptions;
		_pushQueueService = pushQueueService;
		_monitorStoreService = monitorStoreService;
		_runnerNotifyService = runnerNotifyService;

		_hostRunData = new List<HostRunModel>();
	}

	private List<HostRunModel> _hostRunData;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await _taskStoreService.LoadTasks();

		var runnerThread = new Thread(RunTaskQueue);
		runnerThread.IsBackground = false;
		runnerThread.Start();
	}

	private async void RunTaskQueue()
	{
		while (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
		{
			RefreshHostRunData();
			var nextExecution = EvaluateNextRuntimes();
			if (nextExecution.TotalSeconds > 0)
			{
				//wait for the amount or the signal to refresh
				var waitAny = Task.WaitAny(
					Task.Delay(nextExecution),
					_runnerNotifyService.RefreshDataEvent.Await()
					);
				if (waitAny == 1)//indicates the refresh monitor was completed and a monitor list refresh should be invoked
				{
					continue;
				}
			}

			foreach (var value in GetExecutableTaskList().ToArray())
			{
				IStatusMessage statusMessage;
				try
				{
					value.LastExecution = DateTime.Now;
					statusMessage = await InvokeTask(value);
				}
				catch (Exception e)
				{
					statusMessage = FailedStatusMessage.Create(value.Monitor, e, _taskRunnerOptions);
				}

				_pushQueueService.EnqueueMessage(statusMessage, value.Monitor, value.Data);
			}
		}
	}

	private ValueTask<IStatusMessage> InvokeTask(HostRunModel data)
	{
		//future implementation of syntactically different tasks
		if (data.Monitor is IPullMonitor pullTask)
		{
			var timeoutToken = new CancellationTokenSource(data.Data.Timeout);
			var stopToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken.Token,
				_hostApplicationLifetime.ApplicationStopping);
			return pullTask.PullStatusAsync(data.Data, stopToken.Token);
		}

		throw new NotSupportedException("The Task is of an unknown type and cannot be processed.");
	}

	private void RefreshHostRunData()
	{
		_runnerNotifyService.RefreshSignal();
		foreach (var monitorData in _taskStoreService.Tasks
			         .Where(e => !_hostRunData.Select(f => f.Data.Id).Contains(e.Id)))
		{
			var monitor = _monitorStoreService.GetTasks()
				.FirstOrDefault(e => e.Id == monitorData.ReportableMonitorId);
			_hostRunData.Add(new HostRunModel()
			{
				Data = monitorData,
				Monitor = monitor
			});
		}

		foreach (var monitorData in _hostRunData
			         .Where(e => !_taskStoreService.Tasks.Select(f => f.Id).Contains(e.Data.Id)).ToArray())
		{
			_hostRunData.Remove(monitorData);
		}
	}

	private IEnumerable<HostRunModel> GetExecutableTaskList()
	{
		var time = DateTime.Now;
		return _hostRunData
			.Where(e => !e.Data.Disabled)
			.Where(e => e.LastExecution is null || e.LastExecution.Value + e.Data.Interval < time);
	}

	private TimeSpan EvaluateNextRuntimes()
	{
		if (_hostRunData.All(e => e.Data.Disabled))
		{
			return TimeSpan.FromHours(1);
		}

		var time = DateTime.Now;
		return
			_hostRunData
				.Where(e => !e.Data.Disabled)
				.Select(f => (f.LastExecution ?? (time - f.Data.Interval)) + f.Data.Interval)
				.Max() - time;
	}
}