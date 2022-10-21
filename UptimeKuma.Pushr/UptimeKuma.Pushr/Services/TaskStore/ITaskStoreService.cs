using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Services.TaskStore;

public interface ITaskStoreService
{
	IList<MonitorData> Tasks { get; }

	Task AddTask(MonitorData task);
	Task RemoveTask(string id);
	Task UpdateTask(MonitorData task);
	Task LoadTasks();
}

public class MonitorData
{
	public string Id { get; set; }
	public string Title { get; set; }
	public string ReportableMonitorId { get; set; }

	public TimeSpan Interval { get; set; }
	public string PushUrl { get; set; }
	public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);

	public bool Disabled { get; set; }

	public IDictionary<string, string> Data { get; set; }

	public void InfuseUiOptions(IList<IUiOption> fields)
	{
		Title = fields.PopList("NAME").Value;
		Interval = TimeSpan.FromSeconds(int.Parse(fields.PopList("INTERVAL").Value));
		PushUrl = fields.PopList("PUSHURL").Value;
		Data = fields.ToDictionary(e => e.Key, e => e.Value);
	}

	public void PopulateUiOptions(IEnumerable<IUiOption> fields)
	{
		foreach (var field in fields)
		{
			if (Data.TryGetValue(field.Key, out var val))
			{
				field.Value = val;
			}
		}

		fields.First(e => e.Key == "NAME").Value = Title;
		fields.First(e => e.Key == "PUSHURL").Value = PushUrl;
		fields.First(e => e.Key == "INTERVAL").Value = Interval.TotalSeconds.ToString();
	}
}