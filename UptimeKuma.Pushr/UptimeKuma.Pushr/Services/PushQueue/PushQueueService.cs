using System.Collections.Concurrent;
using System.Threading.Channels;
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
		MessageQueue = Channel.CreateUnbounded<MessageQueueItem>();
	}

	public Channel<MessageQueueItem> MessageQueue { get; }

	public void EnqueueMessage(IStatusMessage message, IReportableMonitor monitor, MonitorData monitorData)
	{
		MessageQueue.Writer.TryWrite(new MessageQueueItem(message, monitor, monitorData));
	}

	public Task<HttpResponseMessage> EnqueueMessageAsync(IStatusMessage message, IReportableMonitor monitor,
		MonitorData monitorData)
	{
		var messageQueueItem = new MessageQueueItem(message, monitor, monitorData);
		MessageQueue.Writer.TryWrite(messageQueueItem);
		return messageQueueItem.AwaitDelivery();
	}
}