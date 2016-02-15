using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Converters
{
    /// <summary>
    /// Defines String to DateTime conversion rules.
    /// Can be used in conjunction with the Converter, Parser and ParsingPolicy attributes.
    /// </summary>
    public static class DateTimeConverter
    {
        /// <summary>
        /// CanParse()  implementation of the ConverterAttribute contract.
        /// </summary>
        public static bool CanParse(Type sourceType)
        {
            return sourceType == typeof(DateTime);
        }

        /// <summary>
        /// FromString()  implementation of the ConverterAttribute contract.
        /// </summary>
        public static object FromString(string dateTime, string format)
        {
            return Parse(dateTime, format);
        }

        /// <summary>
        /// ToString() implementation of the ConverterAttribute contract.
        /// </summary>
        public static string ToString(object dateTime, string format)
        {
            return ((DateTime)dateTime).ToString(format);
        }

        /// <summary>
        /// Parse() implementation of the ParserAttribute/ParsingPolicyAttribute contract.
        /// </summary>
        public static object Parse(string dateTime, string format)
        {
            DateTime result;
            bool parsed = DateTime.TryParseExact(dateTime, format, null, System.Globalization.DateTimeStyles.None, out result);
            return parsed ? (object)result : null;
        }
    }
}
