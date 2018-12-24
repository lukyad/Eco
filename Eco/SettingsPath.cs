using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
	public static class SettingsPath
	{
        public const char Separator = '.';

        public const string Root = ".";

        public const char TypeSeparator = ':';


        public static string Combine(string left, string right)
        {
            return $"{left}{Separator}{right}".Trim(Separator);
        }

        public static string AddType(string path, string objectType)
        {
            return $"{path}{TypeSeparator}{objectType}";
        }

		public static string AddType(string path, string objectType, int index)
		{
			return $"{path}[{index}]{TypeSeparator}{objectType}";
        }
	}
}
