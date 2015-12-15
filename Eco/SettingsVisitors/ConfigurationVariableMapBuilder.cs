using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Used by the Eco library to build a dictionary of configuration variables.
    /// The dictionary is then used by the ConfigurationVariableExpander.
    /// 
    /// Throws an exception when 
    /// * an invalid variable name is detected (valid name contains 'word' charcters only)
    /// * a duplicated variable is detected.
    /// </summary>
    public class ConfigurationVariableMapBuilder : ISettingsVisitor
    {
        static readonly Regex _invalidVarChars = new Regex(@"\W");
        readonly Dictionary<string, string> _vars = new Dictionary<string, string>();

        public Dictionary<string, string> Variables { get { return _vars; } }

        // ConfigurationVariableMapBuilder doesn't make any changes per se.
        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRawSettingsType)
        {
            _vars.Clear();
        }

        public void Visit(string settingsNamespace, string settingsPath, object rawSettings)
        {
            if (rawSettings.IsEcoElementOfType<variable>())
                this.RegisterVariable(settingsPath, rawSettings);
        }

        public void Visit(string settingsNamespace, string fieldPath, FieldInfo rawSettingsField, object rawSettings) { }


        void RegisterVariable(string fieldPath, object variable)
        {
            string varName = Eco.variable.GetName(variable);
            string varValue = Eco.variable.GetValue(variable);
            if (String.IsNullOrWhiteSpace(varName)) throw new ConfigurationException("Detected null or empty configuration variable name: path = '{0}'.", fieldPath);
            if (_invalidVarChars.IsMatch(varName)) throw new ConfigurationException("Invalid configuration variable name: '{0}', path = '{1}'.", varName, fieldPath);
            if (_vars.ContainsKey(varName)) throw new ConfigurationException("Duplicated configuration variable: '{0}', path = '{1}'.", varName, fieldPath);
            _vars.Add(varName, varValue);
        }
    }
}
