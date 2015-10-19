using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
    /// <summary>
    /// Used by the Eco library to build a dictionary of configuration variables.
    /// The dictionary is then used by the ConfigurationVariableExpander.
    /// 
    /// Throws an exception when 
    /// * an invalid variable name is detected (valid name contains 'word' charcters only)
    /// * a duplicated variable is detected.
    /// </summary>
    public class ConfigurationVariableMapBuilder : IRawSettingsVisitor
    {
        static readonly Regex _invalidVarChars = new Regex(@"\W");
        readonly Dictionary<string, string> _vars = new Dictionary<string, string>();

        public Dictionary<string, string> Variables { get { return _vars; } }

        // ConfigurationVariableMapBuilder doesn't make any changes per se.
        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRefinedSettingsType)
        {
            _vars.Clear();
        }

        public void Visit(string fieldPath, FieldInfo rawSettingsField, object rawSettings)
        {
            // Raw settings type is generated at runtime. Thus, we do not have a static variable type to use here,
            // so we match variable settings type by name.
            // This might be refactored to leverage attributes in the future.
            if (rawSettingsField.FieldType.Name == typeof(variable).Name)
            {
                this.RegisterVariable(rawSettingsField.GetValue(rawSettings), fieldPath);
            }
            else if (rawSettingsField.FieldType.GetElementType()?.Name == typeof(variable).Name)
            {
                var arr = (Array)rawSettingsField.GetValue(rawSettings);
                if (arr != null)
                {
                    foreach (object variable in arr)
                        this.RegisterVariable(variable, fieldPath);
                }
            }
        }

        void RegisterVariable(object variable, string fieldPath)
        {
            string varName = (string)variable.GetFieldValue("name");
            string varValue = (string)variable.GetFieldValue("value");
            if (String.IsNullOrWhiteSpace(varName)) throw new ConfigurationException("Detected null or empty configuration variable name: path = '{0}'", fieldPath);
            if (_invalidVarChars.IsMatch(varName)) throw new ConfigurationException("Invalid configuration variable name: '{0}', path = '{1}'", varName, fieldPath);
            if (_vars.ContainsKey(varName)) throw new ConfigurationException("Duplicated configuration variable: '{0}', path = '{1}'", varName, fieldPath);
            _vars.Add(varName, varValue);
        }
    }
}
