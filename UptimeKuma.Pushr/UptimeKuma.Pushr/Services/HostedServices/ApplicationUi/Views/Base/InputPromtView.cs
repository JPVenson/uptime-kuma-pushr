using System.Security.Cryptography;
using UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;

public class InputPromtView : ViewBase
{
	public InputPromtView()
	{
		Shortcuts = new Dictionary<string, string>();
	}

	public IDictionary<string, string> Shortcuts { get; set; }

	public string Default { get; set; }
	public string Result { get; set; }

	public override void Render(StringBuilderInterlaced viewRenderer)
	{
		if (!string.IsNullOrWhiteSpace(Description))
		{
			viewRenderer.AppendLine(Description);
		}

		viewRenderer.Append(Title);
		if (Shortcuts.Any())
		{
			viewRenderer.Append("[");
			viewRenderer.Append(string.Join(", ", Shortcuts.Keys));
			viewRenderer.Append("]");
		}

		if (!string.IsNullOrWhiteSpace(Default))
		{
			viewRenderer.Append("(default: \"")
				.Append(Default)
				.Append("\")");
		}

		viewRenderer.Append(":");
	}

	public override async Task Display(bool embedded)
	{
		string input = null;
		do
		{
			await base.Display(embedded);
			input = Console.ReadLine();

			if (string.IsNullOrEmpty(input))
			{
				input = Default;
			}
		} while (string.IsNullOrWhiteSpace(input) && Default == null);

		if (Shortcuts.Any() && Shortcuts.TryGetValue(input, out var shortcut))
		{
			Result = shortcut;
		}
		else
		{
			Result = input;
		}
	}
}