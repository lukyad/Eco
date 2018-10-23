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


		public static string Combine(string left, string right)
		{
            return (left + Separator + right).Trim(Separator);
        }

		public static string Combine(string path, string objectType, int index)
		{
			return $"{path}[{index}]({objectType})";
        }
	}
}
