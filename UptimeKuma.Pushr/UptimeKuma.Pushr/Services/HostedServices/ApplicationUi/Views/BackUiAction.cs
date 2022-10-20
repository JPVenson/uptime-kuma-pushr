namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;

public class BackUiAction : UiAction
{
	private readonly CancellationTokenSource _backRequesTokenSource;

	public BackUiAction(CancellationTokenSource backRequesTokenSource) : base("Q", "Back")
	{
		_backRequesTokenSource = backRequesTokenSource;
	}

	public override void Render(StringBuilderInterlaced viewRenderer)
	{
		_backRequesTokenSource.Cancel();
	}
}