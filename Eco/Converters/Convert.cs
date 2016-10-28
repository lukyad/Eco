using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Eco.Extensions;

namespace Eco.Converters
{
    /// <summary>
    /// An utility class containing helper methods to parse/convert either a single value or array of values from/to a string.
    /// </summary>
    public static class Convert
    {
        public static object FromString<T>(string source, string format, FieldInfo context, Func<string, string, T> ParseValue)
        {
            return context.FieldType.IsArray ? (object)ParseArray(source, format, ParseValue) : (object)ParseValue(source, format);
        }

        static T[] ParseArray<T>(string source, string format, Func<string, string, T> ParseValue)
        {
            return source.Split(',')
                .Select(t => ParseValue(t, format))
                .ToArray();
        }

        public static string ToString<T>(object source, string format, FieldInfo context, Func<T, string, string> ConvertValue)
        {
            return context.FieldType.IsArray ? ConvertArray((T[])source, format, ConvertValue) : ConvertValue((T)source, format);
        }

        static string ConvertArray<T>(T[] source, string format, Func<T, string, string> ConvertValue)
        {
            return source.Select(v => ConvertValue(v, format)).CommaWhiteSpaceSeparated();
        }
    }
}
