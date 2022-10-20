namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views.Base;

public interface IView
{
	string Title { get; set; }
	string Description { get; set; }

	void Render(StringBuilderInterlaced viewRenderer);
	Task Display(bool embedded);
}