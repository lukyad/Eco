using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
	public static class TimeSpanConverter
	{
		private static readonly Dictionary<string, Func<double, TimeSpan>> _fromDoubleMethods = new Dictionary<string, Func<double, TimeSpan>> {
			{  "ms", TimeSpan.FromMilliseconds },
			{ "s", TimeSpan.FromSeconds },
			{ "m", TimeSpan.FromMinutes },
			{ "h", TimeSpan.FromHours },
			{ "d", TimeSpan.FromDays }
		};

		private static readonly Dictionary<string, Func<TimeSpan, double>> _toDoubleMethods = new Dictionary<string, Func<TimeSpan, double>> {
			{ "ms", t => t.TotalMilliseconds },
			{ "s", t => t.TotalSeconds },
			{ "m", t => t.TotalMinutes },
			{ "h", t => t.TotalHours },
			{ "d", t => t.TotalDays }
		};

		public static object FromString(string format, string timeSpan)
		{
			TimeSpan result;
			bool succeed = TryParseTimeSpan(timeSpan, out result);
			if (!succeed)
				throw new ApplicationException(String.Format("Unsupported TimeSpan format: '{0}'", timeSpan));

			return result;
		}

		public static string ToString(string format, object timeSpan)
		{
			Func<TimeSpan, double> ToDouble = _toDoubleMethods[format];
			return String.Format("{0}{1}", ToDouble((TimeSpan)timeSpan), format);
		}

		public static bool TryParseTimeSpan(this string timeSpanStr, out TimeSpan timeSpan)
		{
			timeSpan = default(TimeSpan);

			if (String.IsNullOrEmpty(timeSpanStr))
				return false;

			var parserInfo = _fromDoubleMethods.FirstOrDefault(pair => timeSpanStr.EndsWith(pair.Key));
			if (String.IsNullOrEmpty(parserInfo.Key))
				return false;

			string unitCode = parserInfo.Key;
			string valueStr = timeSpanStr.Substring(0, timeSpanStr.Length - unitCode.Length);
			double value;
			if (!Double.TryParse(valueStr, out value))
				return false;

			timeSpan = _fromDoubleMethods[unitCode](value);

			return true;
		}
	}
}
