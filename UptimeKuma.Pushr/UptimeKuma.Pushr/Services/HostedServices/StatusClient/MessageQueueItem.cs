using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.HostedServices.StatusClient;

public record MessageQueueItem
{
	public MessageQueueItem(IStatusMessage statusMessage, IReportableMonitor reportableMonitor, MonitorData monitorData)
	{
		StatusMessage = statusMessage;
		ReportableMonitor = reportableMonitor;
		MonitorData = monitorData;
	}

	public IStatusMessage StatusMessage { get; private set; }
	public IReportableMonitor ReportableMonitor { get; private set; }
	public MonitorData MonitorData { get; }
}