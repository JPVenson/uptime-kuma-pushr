using UptimeKuma.Pushr.TaskRunner.UiOptions;

namespace UptimeKuma.Pushr.Util;

public static class ListExtensions
{
	/// <summary>Finds the index of the first element matching the predicate, or -1.</summary>
	public static int FindIndex<T>(this IList<T> list, Predicate<T> predicate)
	{
		for (int i = 0; i < list.Count; i++)
			if (predicate(list[i])) return i;
		return -1;
	}

	/// <summary>Removes the first <see cref="IUiOption"/> with the given key from the list and returns it.</summary>
	public static IUiOption PopList(this IList<IUiOption> list, string key)
	{
		var item = list.First(o => o.Key == key);
		list.Remove(item);
		return item;
	}
}
