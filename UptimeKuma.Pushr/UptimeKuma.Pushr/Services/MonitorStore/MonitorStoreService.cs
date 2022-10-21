using Microsoft.Extensions.DependencyInjection;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.MonitorStore;

[SingletonService(typeof(IMonitorStoreService))]
public class MonitorStoreService : IMonitorStoreService
{
	private readonly IServiceProvider _serviceProvider;
	private IEnumerable<IReportableMonitor> _monitors;

	public MonitorStoreService(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public IEnumerable<IReportableMonitor> GetTasks()
	{
		return (_monitors ??= _serviceProvider.GetServices(typeof(IReportableMonitor)).OfType<IReportableMonitor>().ToArray());
	}

	public IReportableMonitor FindTask(string id)
	{
		return (GetTasks()).FirstOrDefault(e => e.Id == id);
	}
}

public interface IMonitorStoreService
{
	IEnumerable<IReportableMonitor> GetTasks();
	IReportableMonitor FindTask(string id);
}