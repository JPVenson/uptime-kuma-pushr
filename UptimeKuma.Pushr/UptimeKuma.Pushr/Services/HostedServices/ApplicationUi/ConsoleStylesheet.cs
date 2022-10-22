using System.Drawing;
using System.Text.RegularExpressions;
using Colorful;
using CConsole = Colorful.Console;
using Console = System.Console;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi
{
	public static class ConsoleStylesheet
	{
		private static Func<string, string> _clearColors;

		public static ConsoleColor ErrorColor = ConsoleColor.Red;
		public static ConsoleColor WarningColor = ConsoleColor.Yellow;
		public static ConsoleColor SuccessColor = ConsoleColor.Green;
		public static ConsoleColor InfoColor = ConsoleColor.Cyan;
		public static ConsoleColor InfoColorAlternate = ConsoleColor.DarkCyan;
		public static ConsoleColor KeyColor = ConsoleColor.Magenta;

		public static StyleSheet ColoredSheet { get; }

		static ConsoleStylesheet()
		{
			_clearColors = s => s;
			var styleSheet = new StyleSheet(CConsole.ForegroundColor);
			//var styleSheet = new StyleSheet(Console.ForegroundColor);
			//foreach (var yieldTransformation in YieldTransformations())
			//{
			//	SetupMarkupPart(styleSheet, yieldTransformation.Item1, yieldTransformation.Item2);
			//}
			ColoredSheet = styleSheet;
		}

		public static IEnumerable<(string, ConsoleColor)> YieldTransformations()
		{
			yield return ("Error", ErrorColor);
			yield return ("Warning", WarningColor);
			yield return ("Success", SuccessColor);
			yield return ("Info", InfoColor);
			yield return ("InfoAlt", InfoColorAlternate);
			yield return ("Key", KeyColor);
		}

		//private static void SetupMarkupPart(StyleSheet styleSheet, string name, Color color)
		//{
		//	var target = $@"<{name}>(.*?)<\/{name}>";
		//	var idLength = $"<{name}>".Length;

		//	string MatchHandler(string originalInput, MatchLocation location, string match)
		//	{
		//		return Regex.Replace(match, target, "$1", RegexOptions.IgnoreCase);
		//	}
			
		//	styleSheet.AddStyle(target, color, MatchHandler);
		//	styleSheet.AddStyle(target.ToLower(), color, MatchHandler);
		//	var old = _clearColors;
		//	_clearColors = s =>
		//	{
		//		s = old(s);
		//		return Regex.Replace(s, target, "$1", RegexOptions.IgnoreCase);
		//	};
		//}
	}
}
