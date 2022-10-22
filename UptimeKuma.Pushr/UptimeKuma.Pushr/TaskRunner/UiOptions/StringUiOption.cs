namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public class StringUiOption : UiOptionBase
{
	public override (bool Valid, string errorText) Validate(string input)
	{
		if (Required && string.IsNullOrWhiteSpace(input))
		{
			return (false, "Input must be more then one character long.");
		}

		return (true, null);
	}
}