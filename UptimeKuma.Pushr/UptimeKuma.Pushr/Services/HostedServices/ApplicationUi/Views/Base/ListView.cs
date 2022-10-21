namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;

public class ListView : ViewBase
{
	public IDictionary<string, string> Items { get; set; }

	public override void Render(StringBuilderInterlaced viewRenderer)
	{
		viewRenderer.AppendLine(Title);
		viewRenderer = viewRenderer.Up();
		if (!Items.Any())
		{
			viewRenderer.AppendInterlacedLine("<info>No Items</info>");
		}

		var maxWidth = Items.Keys.Max(e => e.Length);
		foreach (var item in Items)
		{
			var bullet = item.Key.PadLeft(maxWidth);
			viewRenderer.AppendInterlaced($"<InfoAlt>{bullet}</InfoAlt>")
				.Append(" - ");
			var lines = item.Value.Split('\n');
			viewRenderer.AppendLine(lines[0]);

			foreach (var line in lines.Skip(1))
			{
				var paddedLine = line.PadLeft(line.Length + bullet.Length + 3);
				viewRenderer
					.AppendInterlacedLine(paddedLine);
			}
		}

		viewRenderer.Down();
	}

	public T SelectFrom<T>(IList<T> sourceItems, string inputText)
	{
		var sourceIndex = Items.Keys.FindIndex(e => string.Equals(e, inputText, StringComparison.OrdinalIgnoreCase));
		return sourceIndex != -1 ? sourceItems[sourceIndex] : default;
	}

	public async Task<(T, bool)> DisplaySelector<T>(string text, IList<T> sourceList, bool optional, string allowQuit) where T : class
	{
		var input = new InputPromtView
		{
			Title = text
		};
		if (allowQuit != null)
		{
			input.Shortcuts["q"] = "q";
		}
		T selection = null;
		while (selection is null)
		{
			await input.Display(true);
			if (allowQuit != null)
			{
				if (input.Result == allowQuit)
				{
					return (null, true);
				}
			}

			selection = SelectFrom(sourceList, input.Result);
			if (optional)
			{
				return (selection, false);
			}
		}

		return (selection, false);
	}

	public async Task<T> DisplaySelector<T>(string text, IList<T> sourceList, bool optional) where T : class
	{
		var result = await DisplaySelector(text, sourceList, optional, null);
		return result.Item1;
	}
}