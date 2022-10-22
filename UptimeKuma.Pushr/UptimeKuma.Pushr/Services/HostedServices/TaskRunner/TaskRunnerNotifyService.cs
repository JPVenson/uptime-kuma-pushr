using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.HostedServices.TaskRunner;

[SingletonService(typeof(ITaskRunnerNotifyService))]
public class TaskRunnerNotifyService : ITaskRunnerNotifyService
{
	public TaskRunnerNotifyService()
	{
		RefreshSignal();
		RefreshDataEvent = new AutoResetEvent(false);
	}

	public AutoResetEvent RefreshDataEvent { get; private set; }
	public IEnumerable<(MonitorData Data, StateInfo LastState)> States { get; set; }


	public event EventHandler<MonitorData> StateHasChanged;

	public void SignalRefresh()
	{
		RefreshDataEvent.Set();
	}

	public void RefreshSignal()
	{
	}

	public void SignalTaskStateChange(MonitorData data)
	{
		StateHasChanged?.Invoke(this, data);
	}
}