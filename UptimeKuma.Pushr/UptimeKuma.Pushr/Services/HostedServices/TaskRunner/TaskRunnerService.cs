using Cyotek.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UptimeKuma.Pushr.Services.HostedServices.TaskRunner.Options;
using UptimeKuma.Pushr.Services.MonitorStore;
using UptimeKuma.Pushr.Services.PushQueue;
using UptimeKuma.Pushr.Services.TaskRunnerNotify;
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
		_runnerNotifyService.States = _hostRunData.Select(f => (f.Data, f.LastState));
	}

	private List<HostRunModel> _hostRunData;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await _taskStoreService.LoadTasks();

		var runnerThread = new Thread(RunTaskQueue);
		runnerThread.IsBackground = false;
		runnerThread.Name = "Kuma.TaskRunner";
		runnerThread.Start();
	}

	private async void RunTaskQueue()
	{
		while (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
		{
			await RefreshHostRunData();
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

			foreach (var value in GetPullTaskList().ToArray())
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

	private async ValueTask<IStatusMessage> InvokeTask(HostRunModel hostRunModel)
	{
		//future implementation of syntactically different tasks
		if (hostRunModel.Monitor is IPullMonitor pullTask)
		{
			var timeoutToken = new CancellationTokenSource(hostRunModel.Data.Timeout);
			var stopToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken.Token,
				_hostApplicationLifetime.ApplicationStopping);
			var statusMessage = await pullTask.PullStatusAsync(hostRunModel.Data, stopToken.Token, hostRunModel.LastState);
			UpdateStateInfo(hostRunModel, hostRunModel.LastState.Copy());
			return statusMessage;
		}

		throw new NotSupportedException("The Task is of an unknown type and cannot be processed.");
	}

	private async Task RefreshHostRunData()
	{
		_runnerNotifyService.RefreshSignal();
		foreach (var monitorData in _taskStoreService
			         .Tasks
			         .Where(e => !_hostRunData
				         .Select(f => f.Data.Id)
				         .Contains(e.Id))
			         .Where(e => !e.Disabled))
		{
			var monitor = _monitorStoreService
				.GetTasks()
				.FirstOrDefault(e => e.Id == monitorData.ReportableMonitorId);
			var hostRunModel = new HostRunModel()
			{
				Data = monitorData,
				Monitor = monitor
			};

			hostRunModel.LastState = new StateInfo(CreateLoggerFactory(hostRunModel));
			_hostRunData.Add(hostRunModel);

			if (monitor is IPushMonitor pushMonitor)
			{
				hostRunModel.PushState = await pushMonitor.StartPushState(monitorData,
					_hostApplicationLifetime.ApplicationStopping, state =>
					{
						UpdateStateInfo(hostRunModel, state);
					});
			}
		}

		foreach (var monitorData in _hostRunData
			         .Where(e => !_taskStoreService
				         .Tasks
						 .Select(f => f.Id).Contains(e.Data.Id) || e.Data.Disabled)
			         .ToArray())
		{
			_hostRunData.Remove(monitorData);

			if (monitorData.Monitor is IPushMonitor pushMonitor)
			{
				await pushMonitor.StopPushState(monitorData.PushState);
			}
		}
	}

	private ILoggerFactory CreateLoggerFactory(HostRunModel hostRunModel)
	{
		return new LoggerFactory(new ILoggerProvider[]
		{
			new CachedLoggingProvider(hostRunModel)
		});
	}

	private void UpdateStateInfo(HostRunModel hostRunModel, StateInfo state)
	{
		if (hostRunModel.LastState.State != state.State)
		{
			_runnerNotifyService.SignalTaskStateChange(hostRunModel.Data);
		}

		hostRunModel.LastState = state;
	}

	private IEnumerable<HostRunModel> GetPullTaskList()
	{
		var time = DateTime.Now;
		return FilterMonitors(_hostRunData)
			.Where(e => e.LastExecution is null || e.LastExecution.Value + e.Data.Interval < time);
	}

	private IEnumerable<HostRunModel> FilterMonitors(IEnumerable<HostRunModel> source)
	{
		return source
			.Where(e => e.Monitor is IPullMonitor)
			.Where(e => !e.Data.Disabled)
			.Where(e => e.LastState.State != MonitorState.Broken);
	}

	private TimeSpan EvaluateNextRuntimes()
	{
		if (_hostRunData
		    .Where(e => e.Monitor is IPullMonitor)
		    .All(e => e.Data.Disabled))
		{
			return TimeSpan.FromHours(1);
		}

		var time = DateTime.Now;
		return
			FilterMonitors(_hostRunData)
				.Select(f => (f.LastExecution ?? (time - f.Data.Interval)) + f.Data.Interval)
				.Max() - time;
	}
}

internal class CachedLoggingProvider : ILoggerProvider
{
	public CachedLoggingProvider(HostRunModel hostRunModel)
	{
		Logs = new CircularBuffer<LoggingMessage>(35, true);
		hostRunModel.LoggingProvider = this;
	}

	public void Dispose()
	{
		Logs.Clear();
	}

	public CircularBuffer<LoggingMessage> Logs { get; set; }

	public ILogger CreateLogger(string categoryName)
	{
		return new CachedLogger(this, categoryName);
	}

	private class CachedLogger : ILogger
	{
		private readonly CachedLoggingProvider _cachedLoggingProvider;
		private string _category;

		public CachedLogger(CachedLoggingProvider cachedLoggingProvider, string category)
		{
			_cachedLoggingProvider = cachedLoggingProvider;
			_category = category;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			_cachedLoggingProvider.Logs.Put(new LoggingMessage()
			{
				Message = formatter(state, exception),
				EventId = eventId,
				Exception = exception,
				LogLevel = logLevel,
				Category = _category
			});
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}
	}
}

internal class LoggingMessage
{
	public LogLevel LogLevel { get; set; }
	public string Message { get; set; }
	public EventId EventId { get; set; }
	public Exception Exception { get; set; }
	public string Category { get; set; }
}