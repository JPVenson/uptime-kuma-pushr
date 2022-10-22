using System.Net;
using System.Text;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;
using UptimeKuma.Pushr.Services.HostedServices.StatusClient;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.KumaProxy.Http;

public delegate ValueTask<HttpResponseMessage> HandlePushMessage(IStatusMessage message);

[TransientService(typeof(IReportableMonitor))]
public class HttpPushProxyMonitor : PushMonitorBase
{
	private readonly IPushProxyMonitorHttpServerService _proxyMonitorHttpServerService;
	private readonly IPushQueueService _pushQueueService;
	public const string PORT_OPTIONS_KEY = "Port";
	public const string CODE_OPTIONS_KEY = "Code";

	public HttpPushProxyMonitor(IPushProxyMonitorHttpServerService proxyMonitorHttpServerService,
		IPushQueueService pushQueueService)
		: base("KumaProxy.HttpPushProxyMonitor.V1", "Push Relay", "Opens a Http Server on the set port and relays received pushes. <info>Requires administrator privileges to work.</info>")
	{
		_proxyMonitorHttpServerService = proxyMonitorHttpServerService;
		_pushQueueService = pushQueueService;

		if (!HttpListener.IsSupported)
		{
			IsSupported = false;
			NotSupportedReason = "Missing administrator rights or unsupported Platform.";
		}
	}

	protected override IEnumerable<IUiOption> GetCustomOptions()
	{
		yield return new StringUiOption()
		{
			Required = true,
			Key = PORT_OPTIONS_KEY,
			Name = "Port",
			Default = "80",
			Description = "The Port to open the HTTP server on.",
		};
		yield return new StringUiOption()
		{
			Required = false,
			Key = CODE_OPTIONS_KEY,
			Name = "Kuma Code",
			Default = "",
			Description = "The Kuma code to relay. When unset, takes the same code as from the PushUrl.",
		};
	}

	public override ValueTask<IPushMonitorItem> GetMonitoringItem(MonitorData options,
		CancellationToken cancellationToken,
		PushState setState)
	{
		var port = int.Parse(options.Data[PORT_OPTIONS_KEY]);
		var code = options.Data[CODE_OPTIONS_KEY];
		if (string.IsNullOrWhiteSpace(code))
		{
			code = PushProxyMonitorHttpServerService.GetKumaPushCodeFromUrl(new Uri(options.PushUrl));
		}

		return new ValueTask<IPushMonitorItem>(new HttpServerPushProxy(_proxyMonitorHttpServerService, _pushQueueService, port, code,
			this, options, setState));
	}

	private class HttpServerPushProxy : IPushMonitorItem
	{
		private readonly IPushProxyMonitorHttpServerService _proxyMonitorHttpServerService;
		private readonly IPushQueueService _pushQueueService;
		private readonly int _port;
		private readonly string _code;
		private readonly HttpPushProxyMonitor _httpPushProxyMonitor;
		private readonly MonitorData _options;
		private readonly PushState _setState;

		public HttpServerPushProxy(IPushProxyMonitorHttpServerService proxyMonitorHttpServerService,
			IPushQueueService pushQueueService,
			int port,
			string code,
			HttpPushProxyMonitor httpPushProxyMonitor,
			MonitorData options,
			PushState setState)
		{
			_proxyMonitorHttpServerService = proxyMonitorHttpServerService;
			_pushQueueService = pushQueueService;
			_port = port;
			_code = code;
			_httpPushProxyMonitor = httpPushProxyMonitor;
			_options = options;
			_setState = setState;
		}

		public ValueTask Start()
		{
			_proxyMonitorHttpServerService.AddHandler(_port, _code, HandlePushMessage, _setState);
			return ValueTask.CompletedTask;
		}

		private async ValueTask<HttpResponseMessage> HandlePushMessage(IStatusMessage message)
		{
			return await _pushQueueService.EnqueueMessageAsync(message, _httpPushProxyMonitor, _options);
		}

		public ValueTask Stop()
		{
			_proxyMonitorHttpServerService.RemoveHandler(_port, _code);
			return ValueTask.CompletedTask;
		}
	}
}

public interface IPushProxyMonitorHttpServerService
{
	void AddHandler(int port, string code, HandlePushMessage handler, PushState pushState);
	void RemoveHandler(int port, string code);
}

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
		if (_httpListenerTask != null)
		{
			return;
		}

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

			while (!_stopRequested.IsCancellationRequested)
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
					context.Response.Headers.Add("date", string.Join(';', originalResponse.Content.Headers.GetValues("date")));
					context.Response.Headers.Add("etag", string.Join(';', originalResponse.Content.Headers.GetValues("etag")));
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

			_stopRequested = new CancellationTokenSource();
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
				Handler[port] = null;
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