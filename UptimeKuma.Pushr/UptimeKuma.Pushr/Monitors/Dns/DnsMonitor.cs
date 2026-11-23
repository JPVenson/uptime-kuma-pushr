using System;
using System.Net;
using DnsClient;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.Dns;

[TransientService(typeof(IReportableMonitor))]
public class DnsMonitor : PullMonitorBase
{
    public DnsMonitor()
        : base("DnsMonitor.V1", "Dns Entry Monitor", "Pulls an DNS record from the configured nameserver.")
    {
    }

    public const string HOSTNAME_OPTION_KEY = "HOSTNAME_OPTION_KEY";
    public const string LOOKUP_HOSTNAME_OPTION_KEY = "LOOKUP_HOSTNAME_OPTION_KEY";
    public const string COMPARE_HOSTNAME_OPTION_KEY = "COMPARE_HOSTNAME_OPTION_KEY";
    public const string HOSTNAME_TYPE_OPTION_KEY = "HOSTNAME_TYPE_OPTION_KEY";

    protected override IEnumerable<IUiOption> GetCustomOptions()
    {
        yield return new StringUiOption()
        {
            Required = false,
            Key = HOSTNAME_OPTION_KEY,
            Name = "DNS server",
            Description = "The IP of the DNS server. Default uses the Systems configured DNS.",
        };
        yield return new StringUiOption()
        {
            Required = true,
            Key = LOOKUP_HOSTNAME_OPTION_KEY,
            Name = "DNS record",
            Description = "The hostname to resolve.",
        };
        yield return new StringUiOption()
        {
            Required = true,
            Key = COMPARE_HOSTNAME_OPTION_KEY,
            Name = "Record Expected",
            Description = "The value expected to match the resolved entry.",
        };
        yield return new StringUiOption()
        {
            Required = true,
            Key = HOSTNAME_TYPE_OPTION_KEY,
            Name = "Record Type",
            Description = "The record type to lookup. Supported are A and AAAA",
            SuggestedValues = new Dictionary<string, string>()
            {
                {"0", "A"},
                {"1", "AAAA"}
            }
        };
    }

    public override async ValueTask<IStatusMessage> PullStatusAsync(MonitorData options,
        CancellationToken cancellationToken,
        StateInfo state)
    {
        var dnsServer = options.Data[HOSTNAME_OPTION_KEY];
        var hostname = options.Data[LOOKUP_HOSTNAME_OPTION_KEY];
        var hostnameCompare = options.Data[COMPARE_HOSTNAME_OPTION_KEY];
        var dnsType = options.Data[HOSTNAME_TYPE_OPTION_KEY];

        var lookupClient = new LookupClient(new LookupClientOptions(
            !string.IsNullOrWhiteSpace(dnsServer) ? NameServer.ToNameServer(IPAddress.Parse(dnsServer)) : NameServer.ToNameServer(IPAddress.Parse("127.0.0.0")))
        {
            AutoResolveNameServers = string.IsNullOrWhiteSpace(dnsServer),
        });

        var queryResult = await lookupClient.QueryAsync(hostname, QueryType.A).ConfigureAwait(false);

        var data = dnsType.ToUpper().Trim() switch
        {
            "A" => queryResult.Answers.ARecords().Select(e => e.Address),
            "AAAA" => queryResult.Answers.AaaaRecords().Select(e => e.Address),
            _ => null
        };

        if(data is null)
        {
            return new FailedStatusMessage("Invalid requested message type", "0");            
        }

        if(data.Any(e => string.Equals(e.ToString(), hostnameCompare, StringComparison.InvariantCultureIgnoreCase)))
        {
            return StatusMessage.Ok("found", "1");
        }

        return new FailedStatusMessage("not found", "0");
    }
}