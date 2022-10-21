using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;
using UptimeKuma.Pushr.Services.MonitorStore;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;

public class EditMonitorUiAction : UiAction
{
	private readonly MonitorData _monitorData;
	private readonly IMonitorStoreService _monitorStoreService;
	private readonly ITaskStoreService _taskStoreService;
	private readonly IReportableMonitor _monitor;

	public EditMonitorUiAction(
		MonitorData monitorData,
		IMonitorStoreService monitorStoreService,
		ITaskStoreService taskStoreService) : base("E", "Edit Monitor")
	{
		_monitorData = monitorData;
		_monitorStoreService = monitorStoreService;
		_taskStoreService = taskStoreService;
		_monitor = _monitorStoreService.GetTasks().First(e => e.Id == _monitorData.ReportableMonitorId);
	}

	private ListView _monitorPropertiesListView;

	public override void Render(StringBuilderInterlaced viewRenderer)
	{
		viewRenderer.AppendLine("Please enter the Property you want to change.");
		_monitorPropertiesListView = new ListView()
		{
			Title = $"Properties for '{_monitorData.Title}'",
			Items = _monitor.GetOptionsTemplate().ToNumberdDisplayList(e => e.Name)
		};
		_monitorPropertiesListView.Render(viewRenderer);
	}

	public override async Task Display(bool embedded)
	{
		var fields = _monitor.GetOptionsTemplate().ToList();
		_monitorData.PopulateUiOptions(fields);

		while (true)
		{
			await base.Display(embedded);

			var input = new InputPromtView
			{
				Title = "Select the Property or 'c' to cancel and 's' to save",
				Shortcuts = new Dictionary<string, string>()
				{
					{"c", "cancel"},
					{"s", "save"}
				}
			};
			await input.Display(true);
			if (input.Result == "cancel")
			{
				return;
			}
			if (input.Result == "save")
			{
				break;
			}

			var uiOption = _monitorPropertiesListView.SelectFrom(fields, input.Result);

			var dataInput = new InputPromtView
			{
				Title = "Set new value for " + uiOption.Name,
				Description = uiOption.Description,
				Default = uiOption.Default,
				Shortcuts = uiOption.SuggestedValues ?? new Dictionary<string, string>()
			};

			do
			{
				await dataInput.Display(true);
				var (valid, errorText) = uiOption.Validate(dataInput.Result);
				if (valid)
				{
					uiOption.Value = dataInput.Result;
					break;
				}

				Console.WriteLine(errorText);
			} while (true);
		}
		_monitorData.InfuseUiOptions(fields);

		await _taskStoreService.UpdateTask(_monitorData);
	}
}