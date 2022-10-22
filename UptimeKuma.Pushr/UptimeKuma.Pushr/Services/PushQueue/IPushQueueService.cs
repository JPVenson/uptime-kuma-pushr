using UptimeKuma.Pushr.Services.HostedServices.StatusClient;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.PushQueue;

public interface IPushQueueService
{
	void EnqueueMessage(IStatusMessage message, IReportableMonitor monitor, MonitorData monitorData);
	Task<HttpResponseMessage> EnqueueMessageAsync(IStatusMessage message, IReportableMonitor monitor,
		MonitorData monitorData);

	IEnumerable<MessageQueueItem> GetConsumableEnumerable();
}