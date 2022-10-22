using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Monitors.KumaProxy.Http;

public interface IPushProxyMonitorHttpServerService
{
	void AddHandler(int port, string code, HandlePushMessage handler, PushState pushState);
	void RemoveHandler(int port, string code);
}