using System.Text.RegularExpressions;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.Storage;

[TransientService(typeof(IReportableMonitor))]
public class FilePresencesRegexMonitor : PullMonitorBase
{
	public FilePresencesRegexMonitor() : base("FileExistRegexMonitor.V1", "File Exists", "Checks for a file to exist.")
	{
	}

	public const string FILE_NAME_OPTION_KEY = "DirectoryPath";
	public const string FILE_REGEX_OPTION_KEY = "Regex";

	protected override IEnumerable<IUiOption> GetCustomOptions()
	{
		yield return new StringUiOption()
		{
			Required = true,
			Key = FILE_NAME_OPTION_KEY,
			Name = "Directory Path",
			Description = "The path of the directory to search in",
		};
		yield return new StringUiOption()
		{
			Required = true,
			Key = FILE_REGEX_OPTION_KEY,
			Name = "Regex",
			Description = "The regex used for searching in the folder",
		};
	}

	public override ValueTask<IStatusMessage> PullStatusAsync(MonitorData options, CancellationToken cancellationToken)
	{
		var filename = options.Data[FILE_NAME_OPTION_KEY];
		var regex = new Regex(options.Data[FILE_REGEX_OPTION_KEY]);

		var path = Directory.EnumerateFiles(filename)
			.FirstOrDefault(e => regex.IsMatch(Path.GetFileName(e)));

		if (path is not null)
		{
			return new ValueTask<IStatusMessage>(StatusMessage.Ok("found", "1"));
		}

		return new ValueTask<IStatusMessage>(new FailedStatusMessage("not found", "0"));
	}
}