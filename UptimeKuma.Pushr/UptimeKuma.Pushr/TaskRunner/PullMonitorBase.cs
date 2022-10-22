using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.TaskRunner;

/// <summary>
///		The <see cref="PullMonitorBase"/> Defines a monitor that provides a value at a set time interval.
/// </summary>
public abstract class PullMonitorBase : MonitorBase, IPullMonitor
{
	protected PullMonitorBase(string id, string name, string description) : base(id, name, description)
	{
	}
	
	public abstract ValueTask<IStatusMessage> PullStatusAsync(MonitorData options,
		CancellationToken cancellationToken,
		StateInfo state);
}