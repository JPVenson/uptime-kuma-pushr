using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.KumaProxy;

[TransientService(typeof(IReportableMonitor))]
public class PingMonitor : PullMonitorBase
{
	public PingMonitor() : base("KumaProxy.PingMonitor.V1", "PingMonitor", "Tries to send a ping package.")
	{
	}

	public const string HOSTNAME_OPTION_KEY = "Hostname";
	public const string RANDOM_PAYLOAD_OPTION_KEY = "RandomPayload";
	public const string DONT_FRAGMENT_OPTION_KEY = "DontFragment";
	public const string TIME_TO_LIVE_OPTION_KEY = "TimeToLive";

	protected override IEnumerable<IUiOption> GetCustomOptions()
	{
		yield return new StringUiOption()
		{
			Required = true,
			Key = HOSTNAME_OPTION_KEY,
			Name = "Hostname or IP",
			Description = "The hostname or IP of the destination server.",
		};
		yield return new YesNoUiOption()
		{
			Required = false,
			Key = RANDOM_PAYLOAD_OPTION_KEY,
			Name = "Generate Random Payload",
			Default = "n",
			Description = "Should a random payload for the ping be generated.",
		};
		yield return new YesNoUiOption()
		{
			Required = false,
			Key = DONT_FRAGMENT_OPTION_KEY,
			Default = "n",
			Name = "Dont Fragment",
			Description = "Should the payload be send fragmented or not.",
		};
		yield return new IntUiOption()
		{
			Required = false,
			Key = TIME_TO_LIVE_OPTION_KEY,
			Default = "128",
			Name = "TTL",
			Description = "Gets the number of hops the ping can take before discarded.",
		};
	}

	public override async ValueTask<IStatusMessage> PullStatusAsync(MonitorData options,
		CancellationToken cancellationToken,
		StateInfo state)
	{
		var hostname = options.Data[HOSTNAME_OPTION_KEY];
		var randomPayloadGenerated = options.Data[RANDOM_PAYLOAD_OPTION_KEY];
		var dontFragment = options.Data[DONT_FRAGMENT_OPTION_KEY];
		var ttl = options.Data[TIME_TO_LIVE_OPTION_KEY];

		try
		{
			var ping = new Ping();

			var pingOptions = new PingOptions()
			{
				DontFragment = dontFragment is "y",
				Ttl = int.Parse(ttl)
			};
			var random = randomPayloadGenerated is "y" ? new Random() : new Random(1337);
				
			var buffer = new byte[random.Next(255, 1500)];
			random.NextBytes(buffer);

			var sendPingAsync = await ping.SendPingAsync(hostname, (int)5000, buffer, pingOptions);
			if (sendPingAsync.Status == IPStatus.Success)
			{
				return StatusMessage.Ok("ok", sendPingAsync.RoundtripTime.ToString());
			}

			return new FailedStatusMessage($"Could not send ping because '{sendPingAsync.Status}'", sendPingAsync.RoundtripTime.ToString());
		}
		catch (Exception e)
		{
			return new FailedStatusMessage($"Could not connect because: '{e.Message}'", "0");
		}
	}
}