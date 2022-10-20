using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;
using UptimeKuma.Pushr.Services.MonitorStore;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views
{
	public class EditMonitorView : ViewBase
	{
		private readonly MonitorData _monitorData;
		private readonly IReportableMonitor _monitor;
		private readonly ITaskStoreService _taskStoreService;
		private readonly IMonitorStoreService _monitorStoreService;

		public EditMonitorView(MonitorData monitorData,
			ITaskStoreService taskStoreService,
			IMonitorStoreService monitorStoreService)
		{
			_monitorData = monitorData;
			_taskStoreService = taskStoreService;
			_monitorStoreService = monitorStoreService;
			_monitor = _monitorStoreService.GetTasks().First(e => e.Id == monitorData.ReportableMonitorId);

			SetupTitle();

			_uiActions = SetupUIActions().ToArray();
		}

		private void SetupTitle()
		{
			Title = $"Edit Monitor: {_monitorData.Title} {_monitor.Name}";
			if (_monitorData.Disabled)
			{
				Title += " (Disabled)";
			}
		}

		private IEnumerable<UiAction> SetupUIActions()
		{
			yield return new DeleteMonitorUiAction(_monitorData, _taskStoreService);
			yield return new EditMonitorUiAction(_monitorData, _monitorStoreService, _taskStoreService);
			yield return new ToggleEnabledUiAction(_monitorData, _taskStoreService);
			yield return new BackUiAction(BackRequest);
		}

		private IList<UiAction> _uiActions;
		private ListView _uiActionsList;

		public override void Render(StringBuilderInterlaced viewRenderer)
		{
			SetupTitle();
			viewRenderer.AppendLine(Title);
			_uiActionsList = new ListView()
			{
				Title = "Actions",
				Items = _uiActions.ToDictionary(e => e.ActionName.ToUpper(), e => e.ActionTitle)
			};
			_uiActionsList.Render(viewRenderer);
		}

		public override async Task Display(bool embedded)
		{
			while (!BackRequest.IsCancellationRequested)
			{
				await base.Display(embedded);
				var displaySelector = await _uiActionsList.DisplaySelector("Select action", _uiActions, false);
				await displaySelector.Display(false);
			}
		}
	}
}
