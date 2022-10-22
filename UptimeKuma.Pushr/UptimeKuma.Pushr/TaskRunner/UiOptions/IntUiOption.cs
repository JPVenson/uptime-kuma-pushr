namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public class IntUiOption : UiOptionBase
{
	public override (bool Valid, string errorText) Validate(string input)
	{
		if (Required && !int.TryParse(input, out _))
		{
			return (false, "Input must be a number without fractions.");
		}

		return (true, null);
	}
}