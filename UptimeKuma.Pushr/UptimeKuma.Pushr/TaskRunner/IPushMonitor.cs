using Microsoft.Extensions.Logging;
using UptimeKuma.Pushr.Services.TaskStore;

namespace UptimeKuma.Pushr.TaskRunner;

public delegate void PushState(StateInfo state);

/// <summary>
///		Defines a monitor that pushes its state at its own interval.
/// </summary>
public interface IPushMonitor : IReportableMonitor
{
	///  <summary>
	/// 		Should start an async monitor that pushes its state when necessary.
	///  </summary>
	///  <param name="options">The configuration data as configured by the user.</param>
	///  <param name="cancellationToken">The stop token that indicates either application shutdown or timeout.</param>
	///  <param name="setState">State object to be filled by the monitor.</param>
	///  <returns>A uniq object to identify the push monitor</returns>
	ValueTask<object> StartPushState(MonitorData options, 
		CancellationToken cancellationToken,
		PushState setState);

	/// <summary>
	///		Should stop the async monitor for the particular state.
	/// </summary>
	/// <param name="state">The uniq identifier object for the monitor to stop.</param>
	/// <returns></returns>
	ValueTask StopPushState(object state);
}

public class StateInfo
{
	public StateInfo(ILoggerFactory loggerFactory)
	{
		LoggerFactory = loggerFactory;
	}

	public StateInfo()
	{
		
	}

	public ILoggerFactory LoggerFactory { get; private set; }

	public MonitorState State { get; set; }

	public string BrokenInfoText { get; set; }

	public StateInfo Copy()
	{
		return new StateInfo(LoggerFactory)
		{
			State = State,
			BrokenInfoText = BrokenInfoText
		};
	}
}

public enum MonitorState
{
	/// <summary>
	///		State cannot be determent
	/// </summary>
	Unknown = 0,

	/// <summary>
	///		Monitor is running in expected parameters.
	/// </summary>
	Running = 1,

	/// <summary>
	///		Monitor has stopped working but should be tried later.
	/// </summary>
	Stopped = 2,

	/// <summary>
	///		Monitor has stopped working due to an unresolvable error.
	/// </summary>
	Broken = 3,
}