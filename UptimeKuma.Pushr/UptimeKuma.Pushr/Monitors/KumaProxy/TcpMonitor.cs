using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.KumaProxy
{
	[TransientService(typeof(IReportableMonitor))]
	public class TcpMonitor : PullMonitorBase
	{
		public TcpMonitor() : base("KumaProxy.TcpMonitor.V1", "TcpMonitor IPV4", "Tries to open a TCP connection on the set port.")
		{
		}

		public const string HOSTNAME_OPTION_KEY = "Hostname";
		public const string PORT_OPTION_KEY = "Port";

		protected override IEnumerable<IUiOption> GetCustomOptions()
		{
			yield return new StringUiOption()
			{
				Required = true,
				Key = HOSTNAME_OPTION_KEY,
				Name = "Hostname or IP",
				Description = "The hostname or IP of the destination server.",
			};
			yield return new IntUiOption()
			{
				Required = true,
				Key = PORT_OPTION_KEY,
				Name = "Port",
				Description = "Destination server Port.",
			};
		}

		public override async ValueTask<IStatusMessage> PullStatusAsync(MonitorData options, CancellationToken cancellationToken)
		{
			var hostname = options.Data[HOSTNAME_OPTION_KEY];
			var port = int.Parse(options.Data[PORT_OPTION_KEY]);

			try
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4);
				await socket.ConnectAsync(hostname, port, cancellationToken);
				await socket.DisconnectAsync(false, cancellationToken);
			}
			catch (Exception e)
			{
				return new FailedStatusMessage($"Could not connect because: '{e.Message}'", "0");
			}

			return StatusMessage.Ok("ok", "1");
		}
	}
}
