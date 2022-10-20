namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public interface IUiConfigurableMonitor
{
	IEnumerable<IUiOption> GetOptionsTemplate();
}