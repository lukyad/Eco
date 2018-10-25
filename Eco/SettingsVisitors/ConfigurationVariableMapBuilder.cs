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
    public class ConfigurationVariableMapBuilder : SettingsVisitorBase
    {
        static readonly IVariableProvider[] _variableProviders = GetEcoVariableProviders();

        public ConfigurationVariableMapBuilder() : base(isReversable: true) { }

        public Dictionary<string, Func<string>> Variables { get; } = new Dictionary<string, Func<string>>();

        public override void Initialize(Type rootRawSettingsType)
        {
            Variables.Clear();
            RegisterDynamicVariables();
        }
        public override void Visit(string settingsNamespace, string settingsPath, object rawSettings)
        {
            if (rawSettings.IsEcoElementOfType<variable>())
                this.RegisterVariable(settingsPath, rawSettings);
        }

        static IVariableProvider[] GetEcoVariableProviders()
        {
            return
                typeof(Settings).Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && typeof(IVariableProvider).IsAssignableFrom(t))
                .Select(t => (IVariableProvider)Activator.CreateInstance(t))
                .ToArray();
        }

        void RegisterDynamicVariables()
        {
            foreach (var p in _variableProviders)
            {
                foreach (var v in p.GetVariables())
                {
                    ValidateVariableName(v.Key, varDescription: p.GetType().FullName);
                    Variables.Add(v.Key, v.Value);
                }
            }
        }

        void RegisterVariable(string fieldPath, object variable)
        {
            string varName = Eco.variable.GetName(variable);
            string varValue = Eco.variable.GetValue(variable);
            ValidateVariableName(varName, varDescription: fieldPath);
            Variables.Add(varName, () => varValue);
        }

        void ValidateVariableName(string varName, string varDescription)
        {
            if (String.IsNullOrWhiteSpace(varName)) throw new ConfigurationException("Detected null or empty configuration variable name: path = '{0}'.", varDescription);
            if (Variables.ContainsKey(varName)) throw new ConfigurationException("Duplicate configuration variable: '{0}', path = '{1}'.", varName, varDescription);
        }
    }
}
