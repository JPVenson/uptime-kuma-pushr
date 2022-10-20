using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.TaskRunner;

public abstract class PullMonitorBase : IPullMonitor
{
	protected PullMonitorBase(string id, string name, string description)
	{
		Name = name;
		Description = description;
		Id = id;
	}

	public string Name { get; }
	public string Description { get; }
	public string Id { get; }
	
	public abstract ValueTask<IStatusMessage> PullStatusAsync(MonitorData options,
		CancellationToken cancellationToken);

	protected virtual IEnumerable<IUiOption> GetCustomOptions()
	{
		yield break;
	}

	public IEnumerable<IUiOption> GetOptionsTemplate()
	{
		var uiOptions = new List<IUiOption>()
		{
			new StringUiOption()
			{
				Key = "NAME",
				Name = "Name",
				Description = "The name of this Monitor.",
				Required = true
			},
			new IntUiOption()
			{
				Key = "INTERVAL",
				Name = "Interval",
				Default = "60",
				Description = "The in kuma configured interval in seconds.",
				Required = true
			},
			new UrlUiOption()
			{
				Key = "PUSHURL",
				Name = "Push URL",
				Description =
					"The Url from kuma. Should look something like this: \"https://MY-DOMAIN/api/push/SOMECODE?status=up&msg=OK&ping=\".",
				Required = true
			}
		};
		uiOptions.AddRange(GetCustomOptions());
		return uiOptions;
	}
}