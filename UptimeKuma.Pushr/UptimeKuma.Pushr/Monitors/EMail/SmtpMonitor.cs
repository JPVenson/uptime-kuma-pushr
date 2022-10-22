﻿using System.Diagnostics;
using MailKit.Net.Smtp;
using ServiceLocator.Attributes;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Monitors.EMail
{
	[TransientService(typeof(IReportableMonitor))]
	public class SmtpMonitor : PullMonitorBase
	{
		public SmtpMonitor() : base("SmtpMonitor.V1", "SMTP Monitor", "Tests the connectivity to an SMTP server.")
		{
		}

		public const string HOSTNAME_OPTION_KEY = "Hostname";
		public const string PORT_OPTION_KEY = "Port";
		public const string USE_SSL_OPTION_KEY = "UseSSL";

		public const string USERNAME_OPTION_KEY = "Username";
		public const string PASSWORD_OPTION_KEY = "Password";

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
			yield return new YesNoUiOption()
			{
				Required = false,
				Key = USE_SSL_OPTION_KEY,
				Default = "y",
				Name = "Use SSL",
				Description = "Should SSL be used to establish a connection.",
			};
			yield return new StringUiOption()
			{
				Required = true,
				Key = USERNAME_OPTION_KEY,
				Name = "Username",
				Description = "The Username to authenticate.",
			};
			yield return new StringUiOption()
			{
				Required = true,
				Key = PASSWORD_OPTION_KEY,
				Name = "Password",
				Description = "The password to authenticate. WARNING! Stored as cleartext.",
			};
		}

		public override async ValueTask<IStatusMessage> PullStatusAsync(MonitorData options, 
			CancellationToken cancellationToken,
			StateInfo state)
		{
			var hostname = options.Data[HOSTNAME_OPTION_KEY];
			var port = int.Parse(options.Data[PORT_OPTION_KEY]);
			var useSSL = options.Data[USE_SSL_OPTION_KEY] is "y";

			var smtpClient = new SmtpClient();
			var time = new Stopwatch();
			try
			{
				time.Start();
				await smtpClient.ConnectAsync(hostname, port, useSSL, cancellationToken);
				time.Stop();
			}
			catch (Exception e)
			{
				return new FailedStatusMessage($"Could not connect to the smtp server because '{e.Message}'", "0");
			}

			var username = options.Data[USERNAME_OPTION_KEY];
			var password = options.Data[PASSWORD_OPTION_KEY];
			try
			{
				time.Start();
				await smtpClient.AuthenticateAsync(username, password, cancellationToken);
				time.Stop();
			}
			catch
			{
				return new FailedStatusMessage($"Could not authenticate.", "0");
			}
			finally
			{
				await smtpClient.DisconnectAsync(true, cancellationToken);
			}

			return StatusMessage.Ok("up", time.ElapsedMilliseconds.ToString());
		}
	}
}
