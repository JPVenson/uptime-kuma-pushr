using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Monitors.KumaProxy.Http;

public delegate ValueTask<HttpResponseMessage> HandlePushMessage(IStatusMessage message);