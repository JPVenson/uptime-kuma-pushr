using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.HostedServices.TaskRunner;

internal class HostRunModel
{
	public HostRunModel()
	{
		
	}

	public MonitorData Data { get; set; }
	public DateTime? LastExecution { get; set; }
	public IReportableMonitor Monitor { get; set; }
}