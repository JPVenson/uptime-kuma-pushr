using System.Collections.Concurrent;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.HostedServices.StatusClient;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.PushQueue;

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

	public Task<HttpResponseMessage> EnqueueMessageAsync(IStatusMessage message, IReportableMonitor monitor,
		MonitorData monitorData)
	{
		var messageQueueItem = new MessageQueueItem(message, monitor, monitorData);
		MessageQueue.Add(messageQueueItem);
		return messageQueueItem.AwaitDelivery();
	}

	public IEnumerable<MessageQueueItem> GetConsumableEnumerable()
	{
		return MessageQueue.GetConsumingEnumerable();
	}
}