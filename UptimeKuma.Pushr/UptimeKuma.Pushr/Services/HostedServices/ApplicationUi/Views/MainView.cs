using System.Security.Cryptography;
using Colorful;
using Microsoft.Extensions.Hosting;
using UptimeKuma.Pushr.Services.ActivatorServ;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;
using UptimeKuma.Pushr.Services.MonitorStore;
using UptimeKuma.Pushr.Services.TaskRunnerNotify;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using Console = Colorful.Console;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views
{
	public class MainView : ViewBase
	{
		private readonly ITaskStoreService _taskStoreService;
		private readonly IMonitorStoreService _monitorStoreService;
		private readonly ActivatorService _activatorService;
		private readonly IHostApplicationLifetime _hostApplicationLifetime;
		private readonly ITaskRunnerNotifyService _taskRunnerNotifyService;

		public MainView(ITaskStoreService taskStoreService,
			IMonitorStoreService monitorStoreService,
			ActivatorService activatorService,
			IHostApplicationLifetime hostApplicationLifetime,
			ITaskRunnerNotifyService taskRunnerNotifyService)
		{
			_taskStoreService = taskStoreService;
			_monitorStoreService = monitorStoreService;
			_activatorService = activatorService;
			_hostApplicationLifetime = hostApplicationLifetime;
			_taskRunnerNotifyService = taskRunnerNotifyService;
			_taskRunnerNotifyService.StateHasChanged += _taskRunnerNotifyService_StateHasChanged;
			Title = "Main Menu";
			_uiActions = SetupUiActions().ToArray();
		}

		private void _taskRunnerNotifyService_StateHasChanged(object sender, MonitorData e)
		{
			if (StateWindow != default)
			{
				var oldState = (Console.CursorLeft, Console.CursorTop);
				Console.CursorLeft = StateWindow.CursorLeft;
				Console.CursorTop = StateWindow.CursorTop;

				var writer = new StringBuilderInterlaced();
				writer.Append("<info>The state of one or more monitors has changed. Type r to refresh</info>");
				writer.WriteToConsole();

				Console.CursorLeft = oldState.CursorLeft;
				Console.CursorTop = oldState.CursorTop;
			}
		}

		private IEnumerable<UiAction> SetupUiActions()
		{
			yield return _activatorService.ActivateType<AddNewMonitorUiAction>();
			yield return _activatorService.ActivateType<RefreshUiAction>();
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
					if (!e.Disabled)
					{
						title += "\nState: ";
						var state = _taskRunnerNotifyService.States.FirstOrDefault(f => f.Data.Id == e.Id).LastState?.State 
						            ?? MonitorState.Unknown;
						title += BuildStateDisplay(state);
					}
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

		public static string BuildStateDisplay(MonitorState state)
		{
			switch (state)
			{
				case MonitorState.Unknown:
					return $"{MonitorState.Unknown}";
					break;
				case MonitorState.Running:
					return $"<success>{MonitorState.Running}</success>";
					break;
				case MonitorState.Stopped:
					return $"<warning>{MonitorState.Stopped}</warning>";
					break;
				case MonitorState.Broken:
					return $"<error>{MonitorState.Broken}</error>";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override async Task Display(bool embedded)
		{
			var concreteValue = new Figlet(FigletFont.Default).ToAscii("Kuma Pusr").ConcreteValue;
			System.Console.WriteLine(concreteValue);
			StateWindow = (Console.CursorLeft, Console.CursorTop);

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
					if (mainViewAction is not BackUiAction)
					{
						await base.Display(embedded);
					}
				}
			}

			System.Console.WriteLine("Application will now shutdown...");
			_hostApplicationLifetime.StopApplication();
		}

		public (int CursorLeft, int CursorTop) StateWindow { get; set; }
	}

	public class RefreshUiAction : UiAction
	{
		public RefreshUiAction() : base("R", "Refresh UI")
		{
			Description = "Redraws the Console UI";
		}

		public override void Render(StringBuilderInterlaced viewRenderer)
		{

		}
	}
}
