using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
    /// <summary>
    /// Used by the Eco library to expand environament variables referenced in the configuration file.
    /// Variables expantion is performed on the 'raw settings' (not refined one),
    /// this allows using of variables in any raw settings field of the string type (e.g. fields marked with Converter and Ref attributes).
    /// </summary>
    class EnvironmentVariableExpander : IRawSettingsVisitor
    {
        // Changes made by the EnvironmentVariableExpander are not revocable.
        // i.e. it's not possible to pack expanded strings back to variables.
        public bool IsReversable { get { return false; } }

        public void Initialize(Type rootRefinedSettingsType) { }

        public void Visit(string fieldPath, FieldInfo rawSettingsField, object rawSettings)
        {
            // Skip 'sealed' fields.
            if (rawSettingsField.IsDefined<SealedAttribute>()) return;

            if (rawSettingsField.FieldType == typeof(string))
            {
                string value = (string)rawSettingsField.GetValue(rawSettings);
                if (value != null)
                {
                    string expandedValue = Environment.ExpandEnvironmentVariables(value);
                    rawSettingsField.SetValue(rawSettings, expandedValue);
                }
            }
            else if (rawSettingsField.FieldType == typeof(string[]))
            {
                var arr = (string[])rawSettingsField.GetValue(rawSettings);
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] != null) arr[i] = Environment.ExpandEnvironmentVariables(arr[i]);
                }
            }
        }
    }
}
