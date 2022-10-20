using UptimeKuma.Pushr.Services.TaskStore;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;

public class DeleteMonitorUiAction : UiAction
{
	private readonly MonitorData _monitorData;
	private readonly ITaskStoreService _taskStoreService;

	public DeleteMonitorUiAction(MonitorData monitorData, ITaskStoreService taskStoreService) : base("D", "Delete Monitor")
	{
		_monitorData = monitorData;
		_taskStoreService = taskStoreService;
	}

	public override void Render(StringBuilderInterlaced viewRenderer)
	{
	}

	public override async Task Display(bool embedded)
	{
		await base.Display(embedded);
		var confirmInput = new InputPromtView
		{
			Title = $"Confirm you want to delete the Monitor '{_monitorData.Title}'",
			Shortcuts =
			{
				["y"] = "yes",
				["n"] = "no"
			},
			Default = "n"
		};
		await confirmInput.Display(true);
		if (confirmInput.Result == "yes")
		{
			await _taskStoreService.RemoveTask(_monitorData.Id);
		}
	}
}