using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;
using UptimeKuma.Pushr.Services.TaskStore;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;

public class ToggleEnabledUiAction : UiAction
{
	private readonly ITaskStoreService _taskStoreService;
	private readonly MonitorData _monitorData;
	public ToggleEnabledUiAction(MonitorData monitorData, ITaskStoreService taskStoreService) 
		: base("t", monitorData.Disabled ? "Enable" : "Disable")
	{
		_monitorData = monitorData;
		_taskStoreService = taskStoreService;
	}

	public override void Render(StringBuilderInterlaced viewRenderer)
	{
		_monitorData.Disabled = !_monitorData.Disabled;
		viewRenderer.AppendLine(
			$"The monitor task '{_monitorData.Title}' is now {(_monitorData.Disabled ? "Disabled" : "Enabled")}");
		_taskStoreService.UpdateTask(_monitorData);
	}
}