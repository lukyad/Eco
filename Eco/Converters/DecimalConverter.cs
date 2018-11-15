using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace Eco.Converters
{
    /// <summary>
    /// Defines String to Decimal conversion rules.
    /// Can be used in conjunction with the Converter, Parser and ParsingPolicy attributes.
    /// </summary>
    public static class DecimalConverter
    {
        /// <summary>
        /// CanParse()  implementation of the ConverterAttribute contract.
        /// </summary>
        public static bool CanParse(Type sourceType)
        {
            return sourceType == typeof(decimal);
        }

        /// <summary>
        /// FromString()  implementation of the ConverterAttribute contract.
        /// </summary>
        public static object FromString(string dateTime, string format, FieldInfo context)
        {
            return Convert.FromString(dateTime, format, context, ParseDecimal);
        }

        /// <summary>
        /// ToString() implementation of the ConverterAttribute contract.
        /// </summary>
        public static string ToString(object value, string format, FieldInfo context)
        {
            return Convert.ToString<decimal>(value, format, context, DecimalToString);
        }

        /// <summary>
        /// Parse() implementation of the ParserAttribute/ParsingPolicyAttribute contract.
        /// </summary>
        public static object Parse(string value, string format /*ignored*/, FieldInfo context)
        {
            bool parsed = decimal.TryParse(value, NumberStyles.Any, null, out decimal result);
            return parsed ? (object)result : null;
        }

        static Decimal ParseDecimal(string value, string format /*ignored*/)
        {
            bool parsed = decimal.TryParse(value, NumberStyles.Any, null, out decimal result);
            if (!parsed)
                throw new ApplicationException($"Unable to parse Decimal string `{value}`.");
            return result;
        }

        static string DecimalToString(decimal value, string format)
        {
            return value.ToString(format);
        }
    }
}
