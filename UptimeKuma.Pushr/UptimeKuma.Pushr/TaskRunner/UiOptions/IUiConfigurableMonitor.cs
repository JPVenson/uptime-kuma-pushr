namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

/// <summary>
///		Defines the methods for a monitor to be configured via the UI.
/// </summary>
public interface IUiConfigurableMonitor
{
	/// <summary>
	///		Should return a list of UI options that are needed by the monitor function.
	/// </summary>
	/// <returns></returns>
	IEnumerable<IUiOption> GetOptionsTemplate();
}