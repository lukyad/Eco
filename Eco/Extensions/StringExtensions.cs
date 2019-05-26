using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eco.Extensions
{
    public static class StringExtensions
    {
        public static string[] SplitAndTrim(this string str)
        {
            return SplitAndTrim(str, ',');
        }

        public static string[] SplitAndTrim(this string str, params char[] delimiters)
        {
            return str.Split(delimiters).Select(s => s.Trim()).ToArray();
        }

        public static string ToUpperCamel(this string word)
        {
            return String.Format("{0}{1}",
              word[0].ToString().ToUpperInvariant(),
              word.Substring(1)
          );
        }

        public static string ToLowerCamel(this string word)
        {
            return String.Format("{0}{1}",
              word[0].ToString().ToLowerInvariant(),
              word.Substring(1)
          );
        }
    }
}
