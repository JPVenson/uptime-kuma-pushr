using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.TaskRunner;

public interface IReportableMonitor : IUiConfigurableMonitor
{
	string Name { get; }
	string Description { get; }
	string Id { get; }
}