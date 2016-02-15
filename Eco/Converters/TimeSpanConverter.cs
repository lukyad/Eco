using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Converters
{
    /// <summary>
    /// Defines String to TimeSpan conversion rules.
    /// Can be used in conjunction with the Converter, Parser and ParsingPolicy attributes.
    /// </summary>
    public static class TimeSpanConverter
    {
        static readonly Dictionary<string, Func<double, TimeSpan>> _fromDoubleMethods = new Dictionary<string, Func<double, TimeSpan>> {
            { "ms", TimeSpan.FromMilliseconds }, 
            { "s", TimeSpan.FromSeconds }, 
            { "m", TimeSpan.FromMinutes },
            { "h", TimeSpan.FromHours },
            { "d", TimeSpan.FromDays },
            { "w", value => TimeSpan.FromDays(value * DaysPerWeek) },
            { "y", value => TimeSpan.FromDays(value * DaysPerYear) }
        };

        static readonly Dictionary<string, Func<TimeSpan, double>> _toDoubleMethods = new Dictionary<string, Func<TimeSpan, double>> {
            { "ms", t => t.TotalMilliseconds },
            { "s", t => t.TotalSeconds },
            { "m", t => t.TotalMinutes },
            { "h", t => t.TotalHours },
            { "d", t => t.TotalDays },
            { "w", t => t.TotalDays / DaysPerWeek},
            { "y", t => t.TotalDays / DaysPerYear }
        };

        const double DaysPerWeek = 7;
        const double DaysPerYear = 365;

        /// <summary>
        /// CanParse() implementation of the ConverterAttribute contract.
        /// </summary>
        public static bool CanParse(Type sourceType)
        {
            return sourceType == typeof(TimeSpan);
        }

        /// <summary>
        /// FromString()  implementation of the ConverterAttribute contract.
        /// </summary>
        public static object FromString(string timeSpan, string format)
        {
            return ParseTimeSpan(timeSpan);
        }

        /// <summary>
        /// ToString() implementation of the ConverterAttribute contract.
        /// </summary>
        public static string ToString(object timeSpan, string format)
        {
            Func<TimeSpan, double> ToDouble = _toDoubleMethods[format];
            return String.Format("{0}{1}", ToDouble((TimeSpan)timeSpan), format);
        }

        /// <summary>
        /// Parse() implementation of the ParserAttribute/ParsingPolicyAttribute contract.
        /// </summary>
        public static object Parse(string timeSpan, string format)
        {
            TimeSpan result;
            return TryParseTimeSpan(timeSpan, out result) ? (object)result : null;
        }

        public static TimeSpan ParseTimeSpan(this string timeSpan)
        {
            TimeSpan result;
            bool succeed = TryParseTimeSpan(timeSpan, out result);
            if (!succeed)
                throw new ApplicationException(String.Format("Unsupported TimeSpan format: '{0}'", timeSpan));

            return result;
        }

        public static bool TryParseTimeSpan(this string timeSpanStr, out TimeSpan timeSpan)
        {
            timeSpan = default(TimeSpan);

            if (String.IsNullOrEmpty(timeSpanStr))
                return false;

            var parserInfo = _fromDoubleMethods.FirstOrDefault(pair => timeSpanStr.ToLower().EndsWith(pair.Key));
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
