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

	public override ValueTask<IStatusMessage> PullStatusAsync(MonitorData options,
		CancellationToken cancellationToken,
		StateInfo state)
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

[TransientService(typeof(IReportableMonitor))]
public class DiskSpaceMonitor : PullMonitorBase
{
	public DiskSpaceMonitor() : base("DiskSpaceMonitor.V1", "Disk Space", "Checks the free space of a storage device.")
	{
	}

	public const string FILE_NAME_OPTION_KEY = "DirectoryPath";
	public const string FILE_THRESHOLD_OPTION_KEY = "Threshold";

	protected override IEnumerable<IUiOption> GetCustomOptions()
	{
		yield return new StringUiOption()
		{
			Required = true,
			Key = FILE_NAME_OPTION_KEY,
			Name = "Directory Path",
			Description = "The path of the directory of a device to check",
		};
		yield return new StringUiOption()
		{
			Required = true,
			Key = FILE_THRESHOLD_OPTION_KEY,
			Name = "Threshold %",
			Description = "The threshold in % of used space (90% used for example)",
		};
	}

	public override ValueTask<IStatusMessage> PullStatusAsync(MonitorData options,
		CancellationToken cancellationToken,
		StateInfo state)
	{
		var filename = options.Data[FILE_NAME_OPTION_KEY];
		if (!int.TryParse(options.Data[FILE_THRESHOLD_OPTION_KEY], out var threshold))
		{
			return ValueTask.FromResult<IStatusMessage>(new FailedStatusMessage("Threshold invalid", "0"));
		}


		var drive = DriveInfo.GetDrives().FirstOrDefault(e => filename.StartsWith(e.RootDirectory.FullName));

		if (drive is null)
		{
			return new ValueTask<IStatusMessage>(new FailedStatusMessage("not found", "0"));
		}

		if (!drive.IsReady)
		{
			return new ValueTask<IStatusMessage>(new FailedStatusMessage("Drive is inaccessable", "0"));
		}

		var usage = (float)drive.AvailableFreeSpace / drive.TotalSize * 100;
		if (usage >= threshold)
		{
			return new ValueTask<IStatusMessage>(new FailedStatusMessage("Drive is below threshold", usage.ToString()));
		}

		return new ValueTask<IStatusMessage>(StatusMessage.Ok("found", usage.ToString()));
	}
}