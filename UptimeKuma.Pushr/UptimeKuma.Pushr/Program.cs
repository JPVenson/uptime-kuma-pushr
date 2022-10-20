using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceLocator.Discovery.Option;
using ServiceLocator.Discovery.Service;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi;
using UptimeKuma.Pushr.Services.HostedServices.StatusClient;
using UptimeKuma.Pushr.Services.HostedServices.TaskRunner;

var defaultBuilder = Host.CreateDefaultBuilder();


var hostBuilder = defaultBuilder
	.ConfigureAppConfiguration(e => e.AddCommandLine(args)
		.AddEnvironmentVariables()
		.AddJsonFile("appsettings.json", true))
	.ConfigureServices((context, e) => 
		e.UseServiceDiscovery()
		.FromAssembly(typeof(Program).Assembly)
		.DiscoverOptions(context.Configuration)
		.FromAssembly(typeof(Program).Assembly)
		.LocateServices()
		.AddHostedService<TaskRunnerService>()
		.AddHostedService<KumaPushClientService>()
		.AddHostedService<ApplicationUiService>());

await hostBuilder.RunConsoleAsync(options => options.SuppressStatusMessages = true);