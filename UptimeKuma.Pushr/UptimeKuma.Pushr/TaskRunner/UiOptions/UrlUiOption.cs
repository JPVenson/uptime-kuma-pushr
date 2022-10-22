namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public class UrlUiOption : UiOptionBase
{
	public override (bool Valid, string errorText) Validate(string input)
	{
		if (Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out _))
		{
			return (true, null);
		}
		return (false, "The Uri is not in a correct format.");
	}
}