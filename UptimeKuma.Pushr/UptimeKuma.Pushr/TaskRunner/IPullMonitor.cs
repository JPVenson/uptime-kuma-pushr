using UptimeKuma.Pushr.Services.TaskStore;

namespace UptimeKuma.Pushr.TaskRunner;

public interface IPullMonitor : IReportableMonitor
{
	ValueTask<IStatusMessage> PullStatusAsync(MonitorData options, 
		CancellationToken cancellationToken);
}