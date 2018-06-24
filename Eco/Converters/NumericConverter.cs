using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Eco.Extensions;

namespace Eco.Converters
{
    /// <summary>
    /// Defines String to Number conversion rules.
    /// Can be used in conjunction with the Converter, Parser and ParsingPolicy attributes.
    /// </summary>
    public static class NumericConverter
    {
        static readonly Dictionary<string, decimal> _multipliers = new Dictionary<string, decimal> {
            { "k", 1000 },
            { "m", 1000 * 1000 },
            { "g", 1000 * 1000 * 1000 },
            { "kb", 1024 },
            { "mb", 1024 * 1024 },
            { "gb", 1024 * 1024 * 1024 },
        };
        static readonly Type[] _supportedTypes = new[] {
                typeof(sbyte), typeof(byte), typeof(decimal), typeof(double),
                typeof(float), typeof(int), typeof(long), typeof(short), typeof(uint), typeof(ulong), typeof(ushort) };

        public static bool CanParse(Type sourceType)
        {
            return sourceType.IsArray ?
                _supportedTypes.Contains(sourceType.GetElementType()) :
                _supportedTypes.Contains(sourceType);
        }

        /// <summary>
        /// FromString()  implementation of the ConverterAttribute contract.
        /// </summary>
        public static object FromString(string value, string format, FieldInfo context)
        {
            return Parse(value, format, context);
        }

        /// <summary>
        /// ToString() implementation of the ConverterAttribute contract.
        /// </summary>
        public static string ToString(object value, string multiplier, FieldInfo context)
        {
            if (context.FieldType.IsArray)
            {
                var array = (Array)value;
                var values = new string[array.Length];
                for (int i = 0; i < array.Length; i++)
                    values[i] = NumericToString((decimal)System.Convert.ChangeType(array.GetValue(i), typeof(decimal)), multiplier);
                return values.CommaWhiteSpaceSeparated();
            }
            else
                return Convert.ToString<decimal>(value, multiplier, context, NumericToString);
        }

        /// <summary>
        /// Parse() implementation of the ParserAttribute/ParsingPolicyAttribute contract.
        /// </summary>
        public static object Parse(string value, string format, FieldInfo context)
        {
            if (context.FieldType.IsArray)
            {
                var values = value.SplitAndTrim();
                var elementType = context.FieldType.GetElementType();
                var result = Array.CreateInstance(elementType, values.Length);
                for (int i = 0; i < values.Length; i++)
                {
                    if (!TryParseDecimal(values[i], out decimal d))
                        return null;
                    result.SetValue(System.Convert.ChangeType(d, elementType), i);
                }
                return result;
            }
            else
            {
                return TryParseDecimal(value, out decimal result) ? (object)result : null;
            }
        }

        static string NumericToString(decimal value, string multiplier)
        {
            if (multiplier == null) return value.ToString();
            if (!_multipliers.ContainsKey(multiplier.ToLower())) throw new ConfigurationException("Unsupported number multiplier: '{0}'.", multiplier);
            return String.Format("{0}{1}", value * _multipliers[multiplier], multiplier);
        }

        public static decimal ParseDecimal(this string number)
        {
            decimal result;
            bool succeed = TryParseDecimal(number, out result);
            if (!succeed)
                throw new ConfigurationException("Unsupported number multiplier: '{0}'.", number);

            return result;
        }

        static decimal ParseDecimal(string number, string format)
        {
            return ParseDecimal(number);
        }

        public static bool TryParseDecimal(this string number, out decimal value)
        {
            value = default(decimal);
            if (String.IsNullOrEmpty(number))
                return false;
            // First try the native parsing method.
            if (Decimal.TryParse(number, out value))
                return true;
            // Try custom parsing rules.
            // Get number multiplier (ie, k, Mb, etc)
            var multiplierInfo = _multipliers.FirstOrDefault(pair => number.ToLower().EndsWith(pair.Key));
            if (String.IsNullOrEmpty(multiplierInfo.Key))
                return false;
            // Try parse the rest of the string using the native method.
            string multiplier = multiplierInfo.Key;
            string valueStr = number.Substring(0, number.Length - multiplier.Length);
            if (!Decimal.TryParse(valueStr, out value))
                return false;
            // Final result.
            value *= _multipliers[multiplier];
            return true;
        }
    }
}
