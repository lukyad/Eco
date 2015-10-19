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
    public static class NumberConverter
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
            return _supportedTypes.Contains(sourceType);
        }

        /// <summary>
        /// FromString()  implementation of the ConverterAttribute contract.
        /// </summary>
        public static object FromString(string number, string notUsed)
        {
            return ParseDecimal(number);
        }

        /// <summary>
        /// ToString() implementation of the ConverterAttribute contract.
        /// </summary>
        public static string ToString(object number, string multiplier)
        {
            decimal value = (decimal)number;
            if (!_multipliers.ContainsKey(multiplier.ToLower())) throw new ConfigurationException("Unsupported number multiplier: '{0}'", multiplier);
            return String.Format("{0}{1}", value * _multipliers[multiplier], multiplier);
        }

        /// <summary>
        /// Parse() implementation of the ParserAttribute/ParsingPolicyAttribute contract.
        /// </summary>
        public static object Parse(string number, string notUsed)
        {
            decimal result;
            return TryParseDecimal(number, out result) ? (object)result : null;
        }

        public static decimal ParseDecimal(this string number)
        {
            decimal result;
            bool succeed = TryParseDecimal(number, out result);
            if (!succeed)
                throw new ConfigurationException("Unsupported number multiplier: '{0}'", number);

            return result;
        }

        public static bool TryParseDecimal(this string number, out decimal value)
        {
            value = default(decimal);

            if (String.IsNullOrEmpty(number))
                return false;

            var multiplierInfo = _multipliers.FirstOrDefault(pair => number.ToLower().EndsWith(pair.Key));
            if (String.IsNullOrEmpty(multiplierInfo.Key))
                return false;

            string multiplier = multiplierInfo.Key;
            string valueStr = number.Substring(0, number.Length - multiplier.Length);
            if (!Decimal.TryParse(valueStr, out value))
                return false;

            value *= _multipliers[multiplier];

            return true;
        }
    }
}
