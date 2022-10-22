using System.Net;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.PushQueue;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.KumaProxy.Http;

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