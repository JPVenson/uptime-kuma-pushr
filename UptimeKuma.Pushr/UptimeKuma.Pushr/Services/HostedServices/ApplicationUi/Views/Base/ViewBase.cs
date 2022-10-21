namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;

public abstract class ViewBase : IView
{
	protected ViewBase()
	{
		BackRequest = new CancellationTokenSource();
	}

	public string Title { get; set; }
	public string Description { get; set; }

	public CancellationTokenSource BackRequest { get; set; }

	public abstract void Render(StringBuilderInterlaced viewRenderer);

	public virtual async Task Display(bool embedded)
	{
		var ui = new StringBuilderInterlaced();
		if (!embedded)
		{
			Console.Title = "Uptime-Kuma Pusr - " + Title;
			ui.AppendLine().AppendLine();
		}
		Render(ui);
		ui.WriteToConsole();
	}
}