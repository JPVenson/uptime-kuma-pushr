using UptimeKuma.Pushr.Services.TaskStore;

namespace UptimeKuma.Pushr.TaskRunner;

public abstract class PushMonitorBase : MonitorBase, IPushMonitor
{
	protected PushMonitorBase(string id, string name, string description) : base(id, name, description)
	{

	}

	public abstract ValueTask<IPushMonitorItem> GetMonitoringItem(MonitorData options,
		CancellationToken cancellationToken,
		PushState setState);

	public async ValueTask<object> StartPushState(MonitorData options,
		CancellationToken cancellationToken,
		PushState setState)
	{
		var pushMonitorItem = await GetMonitoringItem(options, cancellationToken, setState);
		await pushMonitorItem.Start();
		return pushMonitorItem;
	}

	public async ValueTask StopPushState(object state)
	{
		var pushMonitorItem = (IPushMonitorItem)state;
		await pushMonitorItem.Stop();
	}
}

/// <summary>
///		A unit of work for a push monitor.
/// </summary>
public interface IPushMonitorItem
{
	ValueTask Start();
	ValueTask Stop();
}