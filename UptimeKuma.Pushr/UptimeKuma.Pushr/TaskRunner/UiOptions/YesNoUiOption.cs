namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public class YesNoUiOption : UiOptionBase
{
	public YesNoUiOption()
	{
		SuggestedValues = new Dictionary<string, string>()
		{
			{ "n", "no" },
			{ "y", "yes" }
		};
	}

	public override (bool Valid, string errorText) Validate(string input)
	{
		if ((!Required && string.IsNullOrEmpty(input)) || (input is "y" or "n" or "yes" or "no") )
		{
			return (true, null);
		}
		return (false, "Input must be either y or n.");
	}
}