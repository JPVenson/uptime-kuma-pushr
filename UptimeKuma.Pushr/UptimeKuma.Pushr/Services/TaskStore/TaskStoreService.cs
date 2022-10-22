using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;
using UptimeKuma.Pushr.Services.TaskRunnerNotify;
using UptimeKuma.Pushr.Services.TaskStore.Options;

namespace UptimeKuma.Pushr.Services.TaskStore;

[SingletonService(typeof(ITaskStoreService))]
public class TaskStoreService : ITaskStoreService
{
	private readonly IOptions<TaskStoreOptions> _taskStoreOptions;
	private readonly ITaskRunnerNotifyService _runnerNotifyService;

	public TaskStoreService(IOptions<TaskStoreOptions> taskStoreOptions,
		ITaskRunnerNotifyService runnerNotifyService)
	{
		_taskStoreOptions = taskStoreOptions;
		_runnerNotifyService = runnerNotifyService;
		Tasks = new List<MonitorData>();
	}

	public IList<MonitorData> Tasks { get; private set; }

	public Task AddTask(MonitorData task)
	{
		Tasks.Add(task);
		_runnerNotifyService.SignalRefresh();
		return SerializeTaskList();
	}

	public Task RemoveTask(string id)
	{
		Tasks.RemoveAt(Tasks.FindIndex(f => f.Id == id));
		_runnerNotifyService.SignalRefresh();
		return SerializeTaskList();
	}

	public Task UpdateTask(MonitorData task)
	{
		var findIndex = Tasks.FindIndex(f => f.Id == task.Id);
		Tasks.RemoveAt(findIndex);
		_runnerNotifyService.SignalRefresh();
		return AddTask(task);
	}

	public async Task LoadTasks()
	{
		await using var fs = new FileStream(_taskStoreOptions.Value.Location, FileMode.OpenOrCreate);
		using var reader = new StreamReader(fs, Encoding.UTF8);
		var jsonSerializer = JsonSerializer.Create();
		Tasks = jsonSerializer.Deserialize(reader, typeof(List<MonitorData>)) as List<MonitorData> ?? new List<MonitorData>();
	}

	private async Task SerializeTaskList()
	{
		await using var fs = new FileStream(_taskStoreOptions.Value.Location, FileMode.Create);
		await using var writer = new StreamWriter(fs, Encoding.UTF8);
		var jsonSerializer = JsonSerializer.Create();
		jsonSerializer.Serialize(writer, Tasks);
	}
}