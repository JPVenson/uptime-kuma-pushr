using System.Net;

namespace UptimeKuma.Pushr.TaskRunner.UiOptions;

public class IpUiOption : UiOptionBase
{
	public IpUiOption()
	{
		
	}

	public override (bool Valid, string errorText) Validate(string input)
	{
		if (Required && (IPAddress.TryParse(input, out _)) )
		{
			return (false, "Input must be a valid IPV4 address");
		}

		return (true, null);
	}
}