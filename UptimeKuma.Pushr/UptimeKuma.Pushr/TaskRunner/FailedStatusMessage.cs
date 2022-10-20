using Microsoft.Extensions.Options;
using UptimeKuma.Pushr.Services.HostedServices.TaskRunner.Options;

namespace UptimeKuma.Pushr.TaskRunner;

public record FailedStatusMessage : IStatusMessage
{
	public FailedStatusMessage(string message, string ping)
	{
		Status = "down";
		Message = message;
		Ping = ping;
	}

	public string Status { get; }
	public string Message { get; }
	public string Ping { get; }

	public static IStatusMessage Create(IReportableMonitor key, 
		Exception exception,
		IOptions<TaskRunnerOptions> options)
	{
		return new FailedStatusMessage(options.Value.TaskFailedText.Replace("ExceptionText", exception.Message), "0");
	}
}