namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public class OfListUiOption : UiOptionBase
{
	private readonly IEnumerable<string> _listOfOptions;

	public OfListUiOption(IEnumerable<string> listOfOptions)
	{
		_listOfOptions = listOfOptions;
	}

	public override (bool Valid, string errorText) Validate(string input)
	{
		if (Required && (_listOfOptions.Contains(input)) )
		{
			return (false, $"Input must be one of \"{string.Join("\", \"", _listOfOptions)}\"");
		}

		return (true, null);
	}
}