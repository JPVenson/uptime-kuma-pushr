using System.Net;
using System.Text;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Monitors.KumaProxy.Http;

[SingletonService(typeof(IPushProxyMonitorHttpServerService))]
public class PushProxyMonitorHttpServerService : IPushProxyMonitorHttpServerService
{
	public PushProxyMonitorHttpServerService()
	{
		_stopRequested = new CancellationTokenSource();
		HttpServer = new HttpListener();
		Handler = new Dictionary<int, IList<(string code, HandlePushMessage handler, PushState pushState)>>();
	}

	public void BeginListenToHttpService(PushState pushState)
	{
		if (_httpListenerTask is { IsCompleted: false })
		{
			return;
		}
		_stopRequested = new CancellationTokenSource();
		_httpListenerTask = ListenToHttpService(pushState);
	}

	private Task ListenToHttpService(PushState pushState)
	{
		return Task.Factory.StartNew(async () =>
		{
			try
			{
				HttpServer.Start();
			}
			catch (HttpListenerException e)
			{
				if (e.NativeErrorCode == 5)
				{
					pushState(new StateInfo()
					{
						BrokenInfoText = $"{e.Message}: This indicates missing administrator rights. Restart the app as Administrator.",
						State = MonitorState.Broken
					});
				}
				else
				{
					pushState(new StateInfo()
					{
						BrokenInfoText = $"{e.Message}",
						State = MonitorState.Broken
					});
				}
			}


			pushState(new StateInfo()
			{
				State = MonitorState.Running
			});

			while (!_stopRequested.IsCancellationRequested && HttpServer.IsListening)
			{
				var context = await HttpServer.GetContextAsync();
				if (context.Request.HttpMethod != HttpMethod.Get.Method)
				{
					context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
					continue;
				}

				var code = GetKumaPushCodeFromUrl(context.Request.Url); //get the code segment
				var port = context.Request.LocalEndPoint;

				var nameValueCollection = context.Request.QueryString;
				var statusMessage = new StatusMessage(
					nameValueCollection["status"],
					nameValueCollection["msg"],
					nameValueCollection["ping"]);

				if (!Handler.TryGetValue(port.Port, out var handlers))
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
					continue;
				}

				var handler = handlers.FirstOrDefault(e => e.code == code);

				if (handler.handler == null)
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
					continue;
				}

				try
				{
					var originalResponse = await handler.handler(statusMessage);

					context.Response.StatusCode = (int)originalResponse.StatusCode;
					context.Response.ContentEncoding = Encoding.UTF8;
					context.Response.ContentType = originalResponse.Content.Headers.ContentType?.MediaType;
					//context.Response.Headers.Add("date", string.Join(';', originalResponse.Content.Headers.GetValues("date")));
					//context.Response.Headers.Add("etag", string.Join(';', originalResponse.Content.Headers.GetValues("etag")));
					await originalResponse.Content.CopyToAsync(context.Response.OutputStream);
				}
				catch (Exception e)
				{
					handler.pushState(new StateInfo()
					{
						BrokenInfoText = e.Message,
						State = MonitorState.Stopped
					});
					context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
					continue;
				}
				finally
				{
					context.Response.OutputStream.Close();
				}
				handler.pushState(new StateInfo()
				{
					State = MonitorState.Running
				});
			}

		}, TaskCreationOptions.LongRunning);
	}

	private Task _httpListenerTask;
	private CancellationTokenSource _stopRequested;

	public HttpListener HttpServer { get; set; }
	public Dictionary<int, IList<(string code, HandlePushMessage handler, PushState pushState)>> Handler { get; set; }

	public void AddHandler(int port, string code, HandlePushMessage handler, PushState pushState)
	{
		HttpServer.Prefixes.Add($"http://*:{port}/api/push/{code}/");
		if (!Handler.TryGetValue(port, out var items))
		{
			items = new List<(string code, HandlePushMessage handler, PushState pushState)>();
			Handler[port] = items;
		}
		items.Add((code, handler, pushState));

		BeginListenToHttpService(pushState);
	}

	public void RemoveHandler(int port, string code)
	{
		HttpServer.Prefixes.Remove($"http://*:{port}/api/push/{code}/");
		if (Handler.TryGetValue(port, out var items))
		{
			items.RemoveAt(items.FindIndex(e => e.code == code));
			if (items.Count == 0)
			{
				Handler.Remove(port);
			}
		}

		if (!Handler.Any())
		{
			_stopRequested.Cancel();
			HttpServer.Stop();
		}
	}

	public static string GetKumaPushCodeFromUrl(Uri url)
	{
		return url.Segments[3].Trim('/');
	}
}