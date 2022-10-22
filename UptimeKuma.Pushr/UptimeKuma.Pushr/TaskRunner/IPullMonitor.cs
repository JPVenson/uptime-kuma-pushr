using UptimeKuma.Pushr.Services.TaskStore;

namespace UptimeKuma.Pushr.TaskRunner;

/// <summary>
///		The <see cref="IPullMonitor"/> Defines a monitor that provides a value at a set time interval.
/// </summary>
public interface IPullMonitor : IReportableMonitor
{
	///  <summary>
	/// 		Pulls the status for the set monitor.
	///  </summary>
	///  <param name="options">The configuration data as configured by the user.</param>
	///  <param name="cancellationToken">The stop token that indicates either application shutdown or timeout.</param>
	///  <param name="state">State object to be filled by the monitor.</param>
	///  <returns></returns>
	ValueTask<IStatusMessage> PullStatusAsync(MonitorData options, 
		CancellationToken cancellationToken,
		StateInfo state);
}