namespace UptimeKuma.Pushr.TaskRunner;

public interface IStatusMessage
{
	string Status { get; }
	string Message { get; }
	string Ping { get; }
}