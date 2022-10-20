using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Monitors;

[TransientService(typeof(IReportableMonitor))]
public class SystemUpMonitor : PullMonitorBase
{
	public SystemUpMonitor() : base("SystemUptime.V1", "System Uptime", "Sends a ping at the set interval.")
	{
	}

	public override ValueTask<IStatusMessage> PullStatusAsync(MonitorData options, CancellationToken cancellationToken)
	{
		return new ValueTask<IStatusMessage>(StatusMessage.Ok("up", "0"));
	}
}