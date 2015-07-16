using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Attributes
{
	/// <summary>
	/// Indicates that all types from the SettingsTypesNamesapace of the given assembly are settings types.
	/// If SettingsTypesNamesapace is null, then all types, defined in the assebmly, are considered to be settings types.
	/// 
	/// Usage:
	/// Can be applied to any assembly.
	/// 
	/// Compatibility:
	/// Compatible with all other attributes.
	/// </summary>
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
