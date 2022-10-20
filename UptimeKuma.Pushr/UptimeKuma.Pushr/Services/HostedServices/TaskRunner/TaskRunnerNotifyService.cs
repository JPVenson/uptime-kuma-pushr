using ServiceLocator.Attributes;

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

	public void SignalRefresh()
	{
		RefreshDataEvent.Set();
	}

	public void RefreshSignal()
	{
	}
}