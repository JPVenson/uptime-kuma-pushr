namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public interface IUiOption
{
	string Name { get; }
	string Description { get; }
	string Key { get; }
	string Value { get; set; }

	string Default { get; }
	bool Required { get; }

	IDictionary<string, string> SuggestedValues { get; }

	(bool Valid, string errorText) Validate(string input);
}