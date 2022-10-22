using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.Process;

[TransientService(typeof(IReportableMonitor))]
public class NamedProcessMonitor : PullMonitorBase
{
	public NamedProcessMonitor() : base("NamedProcessMonitor.V1", "Named Processes", "Checks for processes to be running.")
	{
	}

	public const string PROCESS_NAME_OPTION_KEY = "ProcessName";
	public const string PROCESS_NUMBER_OPTION_KEY = "ExpectedNoProcesses";

	protected override IEnumerable<IUiOption> GetCustomOptions()
	{
		yield return new StringUiOption()
		{
			Required = true,
			Key = PROCESS_NAME_OPTION_KEY,
			Name = "Process name",
			Description = "The name of the process to search for",
		};
		yield return new IntUiOption()
		{
			Required = false,
			Key = PROCESS_NUMBER_OPTION_KEY,
			Name = "Expected number of Processes",
			Description = "When set, expect the exact number of processes to be present",
		};
	}

	public override ValueTask<IStatusMessage> PullStatusAsync(MonitorData options, 
		CancellationToken cancellationToken,
		StateInfo state)
	{
		var processName = options.Data[PROCESS_NAME_OPTION_KEY];
		var processesByName = System.Diagnostics.Process.GetProcessesByName(processName);

		if (processesByName.Length == 0)
		{
			return ValueTask.FromResult<IStatusMessage>(new FailedStatusMessage($"Process '{processName}' does not run", "0"));
		}

		if (options.Data.TryGetValue(PROCESS_NUMBER_OPTION_KEY, out var noProcesses) && int.TryParse(noProcesses, out var iNoProcesses))
		{
			if (processesByName.Length != iNoProcesses)
			{
				return ValueTask.FromResult<IStatusMessage>(new FailedStatusMessage($"Process '{processName}' does not run '{iNoProcesses}' times but '{processesByName.Length}'", 
					processesByName.Length.ToString()));
			}
		}

		return ValueTask.FromResult(StatusMessage.Ok("Running", processesByName.Length.ToString()));
	}
}