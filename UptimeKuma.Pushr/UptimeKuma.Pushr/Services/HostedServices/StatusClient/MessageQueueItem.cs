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
		_deliveryTaskSource = new TaskCompletionSource<HttpResponseMessage>();
	}

	public IStatusMessage StatusMessage { get; private set; }
	public IReportableMonitor ReportableMonitor { get; private set; }
	public MonitorData MonitorData { get; }

	private TaskCompletionSource<HttpResponseMessage> _deliveryTaskSource;

	public Task<HttpResponseMessage> AwaitDelivery()
	{
		return _deliveryTaskSource.Task;
	}

	public void Delivered(HttpResponseMessage httpResponseMessage)
	{
		_deliveryTaskSource.SetResult(httpResponseMessage);
	}

	public void NotDelivered()
	{
		_deliveryTaskSource.SetCanceled();
	}
}