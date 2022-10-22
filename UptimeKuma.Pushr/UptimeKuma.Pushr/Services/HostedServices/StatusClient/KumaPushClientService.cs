using System.Collections.Specialized;
using System.Web;
using Microsoft.Extensions.Hosting;

namespace UptimeKuma.Pushr.Services.HostedServices.StatusClient;

public class KumaPushClientService : BackgroundService
{
	private readonly IPushQueueService _pushQueueService;

	public KumaPushClientService(IPushQueueService pushQueueService)
	{
		_pushQueueService = pushQueueService;
		_httpClients = new Dictionary<string, HttpClient>();
	}

	private readonly IDictionary<string, HttpClient> _httpClients;
	
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var pushThread = new Thread(RunPushQueue);
		pushThread.IsBackground = true;
		pushThread.Start();
		return Task.CompletedTask;
	}

	private async void RunPushQueue(object obj)
	{
		foreach (var messageQueueItem in _pushQueueService.GetConsumableEnumerable())
		{
			var client = EnsureHttpClient(messageQueueItem.MonitorData.PushUrl, out var uri);

			var nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
			var statusMessage = messageQueueItem.StatusMessage;
			nameValueCollection["status"] = statusMessage.Status;
			nameValueCollection["msg"] = statusMessage.Message;
			nameValueCollection["ping"] = statusMessage.Ping;

			var uriBuilder = new UriBuilder(uri)
			{
				Query = string.Join('&',
					EnumerateNameValueCollection(nameValueCollection)
						.Select(e => HttpUtility.UrlEncode(e.Key) + "=" + HttpUtility.UrlEncode(e.Value)))
			};
			try
			{
				var httpResponseMessage = await client.GetAsync(uriBuilder.Uri);
				messageQueueItem.Delivered(httpResponseMessage);
				//todo add logging
			}
			catch (Exception e)
			{
				//todo add logging
				messageQueueItem.NotDelivered();
			}
		}
	}

	private IEnumerable<(string Key, string Value)> EnumerateNameValueCollection(NameValueCollection collection)
	{
		for (int i = 0; i < collection.Count; i++)
		{
			var key = collection.GetKey(i);
			var value = collection[key];
			yield return (key, value);
		}
	}

	private HttpClient EnsureHttpClient(string pushUrl, out Uri uri)
	{
		uri = new Uri(pushUrl);
		if (_httpClients.TryGetValue(uri.Host, out var client))
		{
			return client;
		}

		client = new HttpClient();
		_httpClients[uri.Host] = client;
		return client;
	}
}