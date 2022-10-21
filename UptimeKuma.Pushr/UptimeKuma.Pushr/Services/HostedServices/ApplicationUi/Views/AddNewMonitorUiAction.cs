using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;
using UptimeKuma.Pushr.Services.MonitorStore;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;

public class AddNewMonitorUiAction : UiAction
{
	private readonly IMonitorStoreService _monitorStoreService;
	private readonly ITaskStoreService _taskStoreService;

	public AddNewMonitorUiAction(IMonitorStoreService monitorStoreService,
		ITaskStoreService taskStoreService) : base("A", "Add Monitor")
	{
		_monitorStoreService = monitorStoreService;
		_taskStoreService = taskStoreService;
	}

	private ListView _monitorTypesListView;

	public override void Render(StringBuilderInterlaced viewRenderer)
	{
		viewRenderer.AppendLine("Please enter the Monitor type you want to add");
		_monitorTypesListView = new ListView()
		{
			Items = _monitorStoreService.GetTasks().ToNumberdDisplayList(e => e.Name)
		};
		_monitorTypesListView.Render(viewRenderer);
	}

	public override async Task Display(bool embedded)
	{
		await base.Display(embedded);
		var taskType = await _monitorTypesListView.DisplaySelector("Select Monitor Type", _monitorStoreService.GetTasks().ToArray(), false);
		var fields = new List<IUiOption>();
		fields.AddRange(taskType.GetOptionsTemplate());

		foreach (var uiOption in fields)
		{
			var input = new InputPromtView
			{
				Title = uiOption.Name,
				Description = uiOption.Description,
				Default = uiOption.Default,
				Shortcuts = uiOption.SuggestedValues ?? new Dictionary<string, string>()
			};

			do
			{
				await input.Display(true);
				var (valid, errorText) = uiOption.Validate(input.Result);
				if (valid)
				{
					uiOption.Value = input.Result;
					break;
				}

				Console.WriteLine(errorText);
			} while (true);
		}

		var monitorData = new MonitorData();
		monitorData.Id = Guid.NewGuid().ToString("D");
		monitorData.ReportableMonitorId = taskType.Id;

		monitorData.InfuseUiOptions(fields);
		await _taskStoreService.AddTask(monitorData);
	}
}