using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SettingsAssemblyAttribute : Attribute
    {
		readonly string _settingsTypesNamesapace;

		public SettingsAssemblyAttribute(string settingsTypesNamesapace = null)
		{
			_settingsTypesNamesapace = settingsTypesNamesapace;
		}

		public string SettingsTypesNamesapace { get { return _settingsTypesNamesapace; } }
    }
}
