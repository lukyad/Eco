using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eco
{
    public static class StringExtensions
    {
        public static string WhiteSpaceSeparated(this IEnumerable<string> strs, int spacesCount = 1)
        {
            return strs.SeparatedBy(new string(' ', spacesCount));
        }

        public static string CommaWhiteSpaceSeparated(this IEnumerable<string> strs)
        {
            return strs.SeparatedBy(", ");
        }

        public static string CommaSeparated(this IEnumerable<string> strs)
        {
            return strs.SeparatedBy(',');
        }

        public static string DotSeparated(this IEnumerable<string> strs)
        {
            return strs.SeparatedBy('.');
        }

        public static string TabSeparated(this IEnumerable<string> strs)
        {
            return strs.SeparatedBy('\t');
        }

        public static string SeparatedBy(this IEnumerable<string> strs, char delimiter)
        {
            return strs.SeparatedBy(delimiter.ToString());
        }

        public static string SeparatedBy(this IEnumerable<string> strs, string delimiter)
        {
            return String.Join(delimiter, strs.ToArray());
        }

        public static string Concat(this IEnumerable<string> strs)
        {
            return String.Concat(strs.ToArray());
        }

        public static string[] SplitAndTrim(this string str)
        {
            return SplitAndTrim(str, ',');
        }

        public static string[] SplitAndTrim(this string str, params char[] delimiters)
        {
            return str.Split(delimiters).Select(s => s.Trim()).ToArray();
        }

        public static string ToCamel(this string word)
        {
            return String.Format("{0}{1}",
              word[0].ToString().ToUpperInvariant(),
              word.Substring(1)
          );
        }
    }
}
