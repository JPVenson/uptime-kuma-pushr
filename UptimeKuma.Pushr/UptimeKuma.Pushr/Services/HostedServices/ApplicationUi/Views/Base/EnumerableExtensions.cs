using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi.Views;

public static class EnumerableExtensions
{
	public static IDictionary<string, string> ToNumberdDisplayList<T>(this IEnumerable<T> items,
		Func<T, string> display)
	{
		var displayList = new Dictionary<string, string>();
		var idx = 0;
		foreach (var item in items)
		{
			displayList[(++idx).ToString()] = display(item);
		}

		return displayList;
	}

	public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> condition)
	{
		var idx = 0;
		foreach (var item in items)
		{
			if (condition(item))
			{
				return idx;
			}

			idx++;
		}

		return -1;
	}

	public static string PopDictionary(this IDictionary<string, string> source, string key)
	{
		if (source.TryGetValue(key, out var val))
		{
			source.Remove(key);
			return val;
		}

		return null;
	}

	public static IUiOption PopList(this IList<IUiOption> source, string key)
	{
		var val = source.FirstOrDefault(e => e.Key == key);
		if (val is null)
		{
			return null;
		}
		source.Remove(val);
		return val;

	}
}