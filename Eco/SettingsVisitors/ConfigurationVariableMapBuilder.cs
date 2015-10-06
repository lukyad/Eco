using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eco.Elements;
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
    class ConfigurationVariableMapBuilder : IRawSettingsVisitor
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
                this.RegisterVariable(rawSettingsField.GetValue(rawSettings));
            }
            else if (rawSettingsField.FieldType.GetElementType()?.Name == typeof(variable).Name)
            {
                var arr = (Array)rawSettingsField.GetValue(rawSettings);
                if (arr != null)
                {
                    foreach (object variable in arr)
                        this.RegisterVariable(variable);
                }
            }
        }

        void RegisterVariable(object variable)
        {
            string varName = (string)variable.GetFieldValue("name");
            string varValue = (string)variable.GetFieldValue("value");
            if (_invalidVarChars.IsMatch(varName)) throw new ConfigurationException("Invalid configuration variable name: '{0}'", varName);
            if (_vars.ContainsKey(varName)) throw new ConfigurationException("Duplicated configuration variable: '{0}'", varName);
            _vars.Add(varName, varValue);
        }
    }
}
