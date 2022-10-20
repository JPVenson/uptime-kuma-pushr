using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.Storage;

[TransientService(typeof(IReportableMonitor))]
public class DirectoryPresencesMonitor : PullMonitorBase
{
	public DirectoryPresencesMonitor() : base("DirectoryExistMonitor.V1", "Directory Exists", "Checks for a Directory to exist.")
	{
	}

	public const string DIRECTORY_NAME_OPTION_KEY = "DirectoryPath";

	protected override IEnumerable<IUiOption> GetCustomOptions()
	{
		yield return new StringUiOption()
		{
			Required = true,
			Key = DIRECTORY_NAME_OPTION_KEY,
			Name = "Directory name",
			Description = "The path of the Directory to search for",
		};
	}

	public override ValueTask<IStatusMessage> PullStatusAsync(MonitorData options, CancellationToken cancellationToken)
	{
		var filename = options.Data[DIRECTORY_NAME_OPTION_KEY];

		if (Directory.Exists(filename))
		{
			return new ValueTask<IStatusMessage>(StatusMessage.Ok("found", "1"));
		}

		return new ValueTask<IStatusMessage>(new FailedStatusMessage("not found", "0"));
	}
}