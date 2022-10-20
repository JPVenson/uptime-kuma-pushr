namespace UptimeKuma.Pushr.Services.HostedServices.TaskRunner;

public interface ITaskRunnerNotifyService
{
	AutoResetEvent RefreshDataEvent { get;}

	public void SignalRefresh();
	public void RefreshSignal();
}