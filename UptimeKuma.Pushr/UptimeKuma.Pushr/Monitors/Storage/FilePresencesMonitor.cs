using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.Storage;

[TransientService(typeof(IReportableMonitor))]
public class FilePresencesMonitor : PullMonitorBase
{
	public FilePresencesMonitor() : base("FileExistMonitor.V1", "File Exists", "Checks for a file to exist.")
	{
	}

	public const string FILE_NAME_OPTION_KEY = "FileName";

	protected override IEnumerable<IUiOption> GetCustomOptions()
	{
		yield return new StringUiOption()
		{
			Required = true,
			Key = FILE_NAME_OPTION_KEY,
			Name = "File name",
			Description = "The path of the file to search for",
		};
	}

	public override ValueTask<IStatusMessage> PullStatusAsync(MonitorData options, 
		CancellationToken cancellationToken,
		StateInfo state)
	{
		var filename = options.Data[FILE_NAME_OPTION_KEY];

		if (File.Exists(filename))
		{
			return new ValueTask<IStatusMessage>(StatusMessage.Ok("found", "1"));
		}

		return new ValueTask<IStatusMessage>(new FailedStatusMessage("not found", "0"));
	}
}