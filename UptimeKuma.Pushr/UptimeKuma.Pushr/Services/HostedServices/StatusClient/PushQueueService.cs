using System.Collections.Concurrent;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.HostedServices.StatusClient;

[SingletonService(typeof(IPushQueueService))]
public class PushQueueService : IPushQueueService
{
	public PushQueueService()
	{
		MessageQueue = new BlockingCollection<MessageQueueItem>();
	}

	public BlockingCollection<MessageQueueItem> MessageQueue { get; }

	public void EnqueueMessage(IStatusMessage message, IReportableMonitor monitor, MonitorData monitorData)
	{
		MessageQueue.Add(new MessageQueueItem(message, monitor, monitorData));
	}

	public IEnumerable<MessageQueueItem> GetConsumableEnumerable()
	{
		return MessageQueue.GetConsumingEnumerable();
	}
}