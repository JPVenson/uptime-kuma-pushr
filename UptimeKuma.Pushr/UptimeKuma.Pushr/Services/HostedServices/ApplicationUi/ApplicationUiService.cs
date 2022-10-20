using Microsoft.Extensions.Hosting;
using UptimeKuma.Pushr.Services.ActivatorServ;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi;

public class ApplicationUiService : BackgroundService
{
	private readonly ActivatorService _activatorService;

	public ApplicationUiService(ActivatorService activatorService)
	{
		_activatorService = activatorService;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var uiThread = new Thread(RunUiLoop);
		uiThread.IsBackground = false;
		uiThread.Start();
		return Task.CompletedTask;
	}

	private async void RunUiLoop(object obj)
	{
		var mainView = _activatorService.ActivateType<MainView>();
		await mainView.Display(false);
	}
}