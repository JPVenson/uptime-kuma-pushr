using ServiceLocator.Discovery.Option;

namespace UptimeKuma.Pushr.Services.TaskStore.Options
{
	[FromConfig("TaskStore")]
	public class TaskStoreOptions
	{
		public string Location { get; set; } = "./tasks.json";
	}
}
