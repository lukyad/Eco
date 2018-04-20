﻿using System;
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
        static readonly IVariableProvider[] _variableProviders = GetEcoVariableProviders();
        readonly Dictionary<string, Func<string>> _vars = new Dictionary<string, Func<string>>();

        public Dictionary<string, Func<string>> Variables { get { return _vars; } }

        // ConfigurationVariableMapBuilder doesn't make any changes per se.
        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRawSettingsType)
        {
            _vars.Clear();
            this.RegisterDynamicVariables();
        }

        public void Visit(string settingsNamespace, string settingsPath, object rawSettings)
        {
            if (rawSettings.IsEcoElementOfType<variable>())
                this.RegisterVariable(settingsPath, rawSettings);
        }

        public void Visit(string settingsNamespace, string fieldPath, FieldInfo rawSettingsField, object rawSettings) { }

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
                    _vars.Add(v.Key, v.Value);
                }
            }
        }

        void RegisterVariable(string fieldPath, object variable)
        {
            string varName = Eco.variable.GetName(variable);
            string varValue = Eco.variable.GetValue(variable);
            ValidateVariableName(varName, varDescription: fieldPath);
            _vars.Add(varName, () => varValue);
        }

        void ValidateVariableName(string varName, string varDescription)
        {
            if (String.IsNullOrWhiteSpace(varName)) throw new ConfigurationException("Detected null or empty configuration variable name: path = '{0}'.", varDescription);
            if (_vars.ContainsKey(varName)) throw new ConfigurationException("Duplicate configuration variable: '{0}', path = '{1}'.", varName, varDescription);
        }
    }
}
