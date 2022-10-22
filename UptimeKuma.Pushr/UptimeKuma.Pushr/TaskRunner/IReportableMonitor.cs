using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.TaskRunner;

public interface IReportableMonitor : IUiConfigurableMonitor
{
	/// <summary>
	///		The name of this Monitor.
	/// </summary>
	string Name { get; }

	/// <summary>
	///		The description of the inner workings for this monitor.
	/// </summary>
	string Description { get; }

	/// <summary>
	///		The uniq identifier for this monitor.
	/// </summary>
	string Id { get; }

	/// <summary>
	///		Should return false if this function is not supported on this platform.
	/// </summary>
	bool IsSupported { get; }

	/// <summary>
	///		Additional text if/why this feature is not supported.
	/// </summary>
	string NotSupportedReason { get; }
}