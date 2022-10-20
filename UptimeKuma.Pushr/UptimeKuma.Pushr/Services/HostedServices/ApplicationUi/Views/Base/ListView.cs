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
		foreach (var item in Items)
		{
			viewRenderer.AppendInterlaced($"<InfoAlt> {item.Key} </InfoAlt>")
				.Append(" - ")
				.AppendLine(item.Value);
		}

		viewRenderer.Down();
	}

	public T SelectFrom<T>(IList<T> sourceItems, string inputText)
	{
		var sourceIndex = Items.Keys.FindIndex(e => string.Equals(e, inputText, StringComparison.OrdinalIgnoreCase));
		return sourceIndex != -1 ? sourceItems[sourceIndex] : default;
	}

	public async Task<T> DisplaySelector<T>(string text, IList<T> sourceList, bool optional) where T : class
	{
		var input = new InputPromtView
		{
			Title = text
		};
		T selection = null;
		while (selection is null)
		{
			await input.Display(true);
			selection = SelectFrom(sourceList, input.Result);
			if (optional)
			{
				return selection;
			}
		}

		return selection;
	}
}