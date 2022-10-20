using ServiceLocator.Discovery.Option;

namespace UptimeKuma.Pushr.Services.HostedServices.TaskRunner.Options;

[FromConfig("TaskRunner")]
public class TaskRunnerOptions
{
	public string TaskFailedText { get; set; } = "The Task has Failed because of {ExceptionText}";
}