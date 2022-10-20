namespace UptimeKuma.Pushr.TaskRunner;

public record StatusMessage : IStatusMessage
{
	public StatusMessage(string status, string message, string ping)
	{
		Status = status;
		Message = message;
		Ping = ping;
	}

	public string Status { get; }
	public string Message { get; }
	public string Ping { get; }

	public static IStatusMessage Ok(string message, string ping)
	{
		return new StatusMessage("up", message, ping);
	}
}