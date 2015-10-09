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

		public const string ArrayIndexFormat = "[{0}]";

		public static string Combine(string left, string right)
		{
			return left + Separator + right;
        }

		public static string Combine(string left, string right, int index)
		{
			return Combine(left, right) + String.Format(ArrayIndexFormat, index);
        }
	}
}
