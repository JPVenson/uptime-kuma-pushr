namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public abstract class UiOptionBase : IUiOption
{
	public string Name { get; set; }
	public string Description { get; set; }
	public string Key { get; set; }
	public string Value { get; set; }
	public bool Required { get; set; }
	public string Default { get; set; }

	public IDictionary<string, string> SuggestedValues { get; set; }

	public abstract (bool Valid, string errorText) Validate(string input);
}