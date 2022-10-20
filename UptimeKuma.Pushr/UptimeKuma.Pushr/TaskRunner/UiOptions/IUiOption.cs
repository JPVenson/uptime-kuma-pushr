namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public interface IUiOption
{
	string Name { get; }
	string Description { get; }
	string Key { get; }
	string Value { get; set; }

	string Default { get; }
	bool Required { get; }

	(bool Valid, string errorText) Validate(string input);
}

public abstract class UiOptionBase : IUiOption
{
	public string Name { get; set; }
	public string Description { get; set; }
	public string Key { get; set; }
	public string Value { get; set; }
	public bool Required { get; set; }
	public string Default { get; set; }

	public abstract (bool Valid, string errorText) Validate(string input);
}

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