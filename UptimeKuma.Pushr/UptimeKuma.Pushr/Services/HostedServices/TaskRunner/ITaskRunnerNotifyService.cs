using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.HostedServices.TaskRunner;

public interface ITaskRunnerNotifyService
{
	AutoResetEvent RefreshDataEvent { get;}
	IEnumerable<(MonitorData Data, StateInfo LastState)> States { get; set; }

	event EventHandler<MonitorData> StateHasChanged;

	public void SignalRefresh();
	public void RefreshSignal();
	void SignalTaskStateChange(MonitorData data);
}