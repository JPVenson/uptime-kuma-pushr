namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;

public abstract class UiAction : ViewBase
{
	protected UiAction(string actionName, string actionTitle)
	{
		ActionTitle = actionTitle;
		ActionName = actionName;
		Title = actionTitle;
	}

	public string ActionTitle { get; private set; }
	public string ActionDescription { get; private set; }
	public string ActionName { get; private set; }
}