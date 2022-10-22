using System.Drawing;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Colorful;
using Console = System.Console;

namespace UptimeKuma.Pushr.Services.HostedServices.ApplicationUi
{
	/// <summary>
	///     Allows building of strings in a interlaced and colored way
	/// </summary>
	public class StringBuilderInterlaced
	{
		private readonly uint _interlacedSpace;

		private readonly StringBuilder _source;
		private readonly bool _transformInterlaced;
		private int _interlacedLevel;

		/// <summary>
		/// </summary>
		/// <param name="transformInterlaced">If true an level will be displaced as <paramref name="intedtSize" /> spaces</param>
		/// <param name="intedtSize">ammount of spaces for each level</param>
		public StringBuilderInterlaced(bool transformInterlaced = false, uint intedtSize = 4)
		{
			_interlacedSpace = intedtSize;
			_transformInterlaced = transformInterlaced;
			_source = new StringBuilder();
			SyncRoot = new object();
		}

		/// <summary>
		///     increases all folloring Text parts by 1
		/// </summary>
		/// <returns></returns>
		public virtual StringBuilderInterlaced Up()
		{
			_interlacedLevel++;
			return this;
		}

		/// <summary>
		///     decreases all folloring Text parts by 1
		/// </summary>
		/// <returns></returns>
		public virtual StringBuilderInterlaced Down()
		{
			if (_interlacedLevel > 0)
			{
				_interlacedLevel--;
			}
			return this;
		}

		/// <summary>
		///     Appends the line.
		/// </summary>
		/// <returns></returns>
		public virtual StringBuilderInterlaced AppendLine()
		{
			return Append(Environment.NewLine);
		}

		private void ApplyLevel()
		{
			var text = "";
			if (_transformInterlaced)
			{
				for (var i = 0; i < _interlacedLevel; i++)
					for (var j = 0; j < _interlacedSpace; j++)
					{
						text += " ";
					}
			}
			else
			{
				for (var i = 0; i < _interlacedLevel; i++)
				{
					text += "\t";
				}
			}
			Add(text);
		}

		private void Add(string text)
		{
			lock (SyncRoot)
			{
				_source.Append(text);
				Count += text.Length;
			}
		}

		/// <summary>
		///     Appends the interlaced line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced AppendInterlacedLine(string value)
		{
			ApplyLevel();
			return AppendLine(value);
		}

		/// <summary>
		///     Appends the interlaced.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced AppendInterlaced(string value)
		{
			ApplyLevel();
			return Append(value);
		}

		/// <summary>
		///     Inserts the specified delete.
		/// </summary>
		/// <param name="del">The delete.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced Insert(Action<StringBuilderInterlaced> del)
		{
			del(this);
			return this;
		}

		/// <summary>
		///     Inserts the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced Insert(StringBuilderInterlaced writer)
		{
			Append(writer._source.ToString());
			return this;
		}

		/// <summary>
		///     Appends the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced Append(string value)
		{
			Add(value);
			return this;
		}

		/// <summary>
		///     Appends the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced AppendLine(string value)
		{
			return Append(value + Environment.NewLine);
		}

		/// <summary>
		///     Appends the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced Append(string value, params object[] values)
		{
			return Append(string.Format(value, values));
		}

		/// <summary>
		///     Appends the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced AppendLine(string value, params object[] values)
		{
			return Append(string.Format(value, values) + Environment.NewLine);
		}

		/// <summary>
		///     Appends the interlaced line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced AppendInterlacedLine(string value, params object[] values)
		{
			return AppendInterlacedLine(string.Format(value, values));
		}

		/// <summary>
		///     Appends the interlaced.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public virtual StringBuilderInterlaced AppendInterlaced(string value, params object[] values)
		{
			return AppendInterlaced(string.Format(value, values));
		}

		public virtual void WriteToConsole()
		{
			var format = _source.ToString();
			//quickfix this is only neede until Colorful Console fixes the Blue issue
			//https://github.com/tomakita/Colorful.Console/issues/88
			//foreach (var yieldTransformation in ConsoleStylesheet.YieldTransformations())
			//{
			//	format = format.Replace($"<{yieldTransformation.Item1}>", "")
			//		.Replace($"</{yieldTransformation.Item1}>", "")
			//		.Replace($"<{yieldTransformation.Item1.ToLower()}>", "")
			//		.Replace($"</{yieldTransformation.Item1.ToLower()}>", "");
			//}

			var colorList = new List<(string, ConsoleColor)>();

			var targetPattern = $@"<(.*)>(.*?)<\/\1>";
			var regex = new Regex(targetPattern, RegexOptions.IgnoreCase);
			var transformations = ConsoleStylesheet.YieldTransformations().ToArray();

			foreach (Match match in regex.Matches(format))
			{
				var keyword = match.Groups[1].Value;
				var innerText = match.Groups[2].Value;

				var transformation =
					transformations.FirstOrDefault(e => e.Item1.Equals(keyword, StringComparison.OrdinalIgnoreCase));
				
				format = format.Replace(match.Value, "{marker}");
				if (transformation != default)
				{
					colorList.Add((innerText, transformation.Item2));
				}
			}

			var parts = format.Split("{marker}");
			for (var index = 0; index < parts.Length; index++)
			{
				var part = parts[index];
				Console.Write(part);
				if (index < colorList.Count)
				{
					var colored = colorList[index];
					var oldForground = Console.ForegroundColor;
					Console.ForegroundColor = colored.Item2;
					Console.Write(colored.Item1, colored.Item2);
					Console.ForegroundColor = oldForground;
				}
			}
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents all text parts without any color
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return _source.ToString();
		}

		/// <summary>
		/// throws NotImplementedException
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// return the Count of all Text-String elements
		/// </summary>
		public int Count { get; private set; }
		/// <summary>
		/// Returns the internal String length
		/// </summary>
		public int Length { get; set; }

		/// <inheritdoc />
		public object SyncRoot { get; private set; }

		/// <inheritdoc />
		public bool IsSynchronized
		{
			get { return Monitor.IsEntered(SyncRoot); }
		}
	}
}
