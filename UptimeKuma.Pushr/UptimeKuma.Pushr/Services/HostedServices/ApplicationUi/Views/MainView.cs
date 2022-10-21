using Colorful;
using Microsoft.Extensions.Hosting;
using UptimeKuma.Pushr.Services.ActivatorServ;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;
using UptimeKuma.Pushr.Services.MonitorStore;
using UptimeKuma.Pushr.Services.TaskStore;
using Console = Colorful.Console;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views
{
	public class MainView : ViewBase
	{
		private readonly ITaskStoreService _taskStoreService;
		private readonly IMonitorStoreService _monitorStoreService;
		private readonly ActivatorService _activatorService;
		private readonly IHostApplicationLifetime _hostApplicationLifetime;

		public MainView(ITaskStoreService taskStoreService,
			IMonitorStoreService monitorStoreService,
			ActivatorService activatorService,
			IHostApplicationLifetime hostApplicationLifetime)
		{
			_taskStoreService = taskStoreService;
			_monitorStoreService = monitorStoreService;
			_activatorService = activatorService;
			_hostApplicationLifetime = hostApplicationLifetime;
			Title = "Main Menu";
			_uiActions = SetupUiActions().ToArray();
		}

		private IEnumerable<UiAction> SetupUiActions()
		{
			yield return _activatorService.ActivateType<AddNewMonitorUiAction>();
			yield return new BackUiAction(BackRequest)
			{
				Description = "Quits the App",
			};
		}

		private ListView _listOfRunningTasks;
		private ListView _mainViewActions;

		private IList<UiAction> _uiActions;

		public override void Render(StringBuilderInterlaced viewRenderer)
		{
			_listOfRunningTasks = new ListView()
			{
				Title = "Running Monitors",
				Items = _taskStoreService.Tasks.ToNumberdDisplayList(e =>
				{
					var title = e.Title;
					if (e.Disabled)
					{
						title += " <warning>(Disabled)</warning>";
					}

					var monitor = _monitorStoreService.FindTask(e.ReportableMonitorId);
					title += $"\n<info>{monitor.Name}</info>";
					return title;
				})
			};
			_listOfRunningTasks.Render(viewRenderer);

			viewRenderer.AppendLine();

			_mainViewActions = new ListView()
			{
				Title = "Actions",
				Items = _uiActions.ToDictionary(e => e.ActionName, e => $"{e.ActionTitle}\n{e.ActionDescription ?? e.Description}")
			};
			_mainViewActions.Render(viewRenderer);
		}

		public override async Task Display(bool embedded)
		{
			var concreteValue = new Figlet(FigletFont.Default).ToAscii("Kuma Pusr").ConcreteValue;
			System.Console.WriteLine(concreteValue);

			await base.Display(embedded);
			while (!BackRequest.IsCancellationRequested)
			{
				var inputSelection = new InputPromtView()
				{
					Title = "Select number to edit or action",
				};
				await inputSelection.Display(true);

				var editMonitor = _listOfRunningTasks.SelectFrom(_taskStoreService.Tasks, inputSelection.Result);
				if (editMonitor is not null)
				{
					var editMonitorUiAction = _activatorService.ActivateType<EditMonitorView>(editMonitor);
					await editMonitorUiAction.Display(false);
					await base.Display(embedded);
					continue;
				}

				var mainViewAction = _mainViewActions.SelectFrom(_uiActions, inputSelection.Result);
				if (mainViewAction is not null)
				{
					await mainViewAction.Display(false);
					await base.Display(embedded);
				}
			}

			_hostApplicationLifetime.StopApplication();
		}
	}
}
