using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Eco.Extensions;

namespace Eco.Converters
{
    /// <summary>
    /// Defines String to TimeSpan conversion rules.
    /// Can be used in conjunction with the Converter, Parser and ParsingPolicy attributes.
    /// </summary>
    public static class TimeSpanConverter
    {
        static readonly Dictionary<string, Func<double, TimeSpan>> _fromDoubleMethods = new Dictionary<string, Func<double, TimeSpan>> {
            { "t", t => TimeSpan.FromTicks(t) },
            { "ms", TimeSpan.FromMilliseconds },
            { "s", TimeSpan.FromSeconds }, 
            { "m", TimeSpan.FromMinutes },
            { "h", TimeSpan.FromHours },
            { "d", TimeSpan.FromDays },
            { "w", value => TimeSpan.FromDays(value * DaysPerWeek) },
            { "y", value => TimeSpan.FromDays(value * DaysPerYear) }
        };

        static readonly Dictionary<string, Func<TimeSpan, double>> _toDoubleMethods = new Dictionary<string, Func<TimeSpan, double>> {
            { "ms", t => t.Ticks },
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
            return
                sourceType == typeof(TimeSpan) ||
                sourceType == typeof(TimeSpan[]);
        }

        /// <summary>
        /// FromString()  implementation of the ConverterAttribute contract.
        /// </summary>
        public static object FromString(string source, string format, FieldInfo context)
        {
            return Converters.Convert.FromString<TimeSpan>(source, format, context, ParseTimeSpan);
        }

        /// <summary>
        /// ToString() implementation of the ConverterAttribute contract.
        /// </summary>
        public static string ToString(object source, string format, FieldInfo context)
        {
            return Converters.Convert.ToString<TimeSpan>(source, format, context, TimeSpanToString);
        }

        public static string TimeSpanToString(TimeSpan source, string format)
        {
            Func<TimeSpan, double> ToDouble;
            if (!_toDoubleMethods.TryGetValue(format, out ToDouble))
                throw new ApplicationException(String.Format("Unsupported time unit: '{0}'. Expected one of the following: '{1}'.", source, _toDoubleMethods.Keys.CommaWhiteSpaceSeparated()));

            return String.Format("{0}{1}", ToDouble(source), format);
        }

        /// <summary>
        /// Parse() implementation of the ParserAttribute/ParsingPolicyAttribute contract.
        /// </summary>
        public static object Parse(string timeSpan, string format, FieldInfo context)
        {
            if (context.FieldType.IsArray)
            {
                var timeSpans = timeSpan.SplitAndTrim();
                var result = new TimeSpan[timeSpans.Length];
                for (int i = 0; i < timeSpans.Length; i++)
                {
                    if (!TryParseTimeSpan(timeSpans[i], out result[i]))
                        return null;
                }
                return result;
            }
            else
            {
                TimeSpan result;
                return TryParseTimeSpan(timeSpan, out result) ? (object)result : null;
            }
        }

        public static TimeSpan ParseTimeSpan(this string timeSpan)
        {
            TimeSpan result;
            bool succeed = TryParseTimeSpan(timeSpan, out result);
            if (!succeed)
                throw new ConfigurationException("Unsupported TimeSpan format: '{0}'", timeSpan);

            return result;
        }

        static TimeSpan ParseTimeSpan(this string timeSpan, string format)
        {
            return ParseTimeSpan(timeSpan);
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
            if (valueStr.Length > 0)
            {
                if (!Double.TryParse(valueStr, out value))
                    return false;
            }
            else
            {
                value = 1;
            }

            timeSpan = _fromDoubleMethods[unitCode](value);

            return true;
        }
    }
}
