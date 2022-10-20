using System.Drawing;
using System.Text.RegularExpressions;
using Colorful;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi
{
	public static class ConsoleStylesheet
	{
		private static Func<string, string> _clearColors;

		public static Color ErrorColor = Color.Red;
		public static Color WarningColor = Color.Yellow;
		public static Color SuccessColor = Color.LawnGreen;
		public static Color InfoColor = Color.Aqua;
		public static Color InfoColorAlternate = Color.MediumAquamarine;
		public static StyleSheet ColoredSheet { get; }

		static ConsoleStylesheet()
		{
			_clearColors = s => s;

			var styleSheet = new StyleSheet(Color.White);
			//var styleSheet = new StyleSheet(Console.ForegroundColor);
			foreach (var yieldTransformation in YieldTransformations())
			{
				SetupMarkupPart(styleSheet, yieldTransformation.Item1, yieldTransformation.Item2);
			}
			ColoredSheet = styleSheet;
		}

		public static IEnumerable<(string, Color)> YieldTransformations()
		{
			yield return ("Error", ErrorColor);
			yield return ("Warning", WarningColor);
			yield return ("Success", SuccessColor);
			yield return ("Info", InfoColor);
			yield return ("InfoAlt", InfoColorAlternate);
		}

		private static void SetupMarkupPart(StyleSheet styleSheet, string name, Color color)
		{
			var target = $@"<{name}>(.*?)<\/{name}>";
			var idLength = $"<{name}>".Length;

			string MatchHandler(string originalInput, MatchLocation location, string match)
			{
				return Regex.Replace(match, target, "$1", RegexOptions.IgnoreCase);
			}

			styleSheet.AddStyle(target, color, MatchHandler);
			styleSheet.AddStyle(target.ToLower(), color, MatchHandler);
			var old = _clearColors;
			_clearColors = s =>
			{
				s = old(s);
				return Regex.Replace(s, target, "$1", RegexOptions.IgnoreCase);
			};
		}
	}
}
