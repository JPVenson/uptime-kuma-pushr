using Microsoft.Extensions.Logging;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.HostedServices.TaskRunner;

internal class HostRunModel
{
	public MonitorData Data { get; set; }
	public DateTime? LastExecution { get; set; }
	public IReportableMonitor Monitor { get; set; }
	public object PushState { get; set; }

	public StateInfo LastState { get; set; }

	public CachedLoggingProvider LoggingProvider { get; set; }
}