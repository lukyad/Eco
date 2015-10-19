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
    /// Used by the Eco library to expand variables in the configuration file.
    /// Variables expantion is performed on the 'raw settings' (not refined one),
    /// this allows using of variables in any raw settings field of the string type (e.g. fields marked with Converter and Ref attributes).
    /// 
    /// If an unknown variable detected then the string is left unchanged (i.e. no expansion is happend)
    /// 
    /// Throws an exception if a circular variable dependency is detected.
    /// </summary>
    public class ConfigurationVariableExpander : IRawSettingsVisitor
    {
        // Mathes variable reference in a string.
        static readonly Regex _variableReferenceRegex = new Regex(@"\$\{(?<varName>\w)\}");
        // Name/value variable pairs.
        readonly Dictionary<string, string> _variables;

        public ConfigurationVariableExpander(Dictionary<string, string> variables)
        {
            _variables = variables;
        }

        // Changes made by the ConfigurationVariableExpander are not revocable.
        // i.e. it's not possible to pack expanded strings back to variables.
        public bool IsReversable { get { return false; } }

        public void Initialize(Type rootRefinedSettingsType) { }

        public void Visit(string fieldPath, FieldInfo rawSettingsField, object rawSettings)
        {
            // Skip 'sealed' fields.
            if (rawSettingsField.IsDefined<SealedAttribute>()) return;
            
            if (rawSettingsField.FieldType == typeof(string))
            {
                var originalString = (string)rawSettingsField.GetValue(rawSettings);
                if (originalString != null)
                {
                    var expandedString = ExpandVariables(originalString, _variables);
                    rawSettingsField.SetValue(rawSettings, expandedString);
                }
            }
            else if (rawSettingsField.FieldType == typeof(string[]))
            {
                string[] arr = (string[])rawSettingsField.GetValue(rawSettings);
                for (int i = 0; i < arr.Length; i++)
                {
                    string originalString = arr[i];
                    if (originalString != null)
                    {
                        var expandedString = ExpandVariables(originalString, _variables);
                        arr[i] = expandedString;
                    }
                }
            }
        }

        static string ExpandVariables(string source, Dictionary<string, string> variables)
        {
            string result = source;
            var expandedVars = new HashSet<string>();
            while (true)
            {
                // Match all variables referenced in the current string.
                MatchCollection varMatches = Regex.Matches(result, @"\$\{(?<varName>\w+)\}");
                if (varMatches.Count == 0) break;

                // Expand matched variables and remember them in a HashSet.
                // Expanded variables should not appear again as this would lead to circular dependency.
                var localExpandedVars = new List<string>();
                foreach (Match m in varMatches)
                {
                    string varName = m.Groups["varName"].Value;
                    // Make sure we do not go into a circular dependency.
                    if (expandedVars.Contains(varName)) throw new ConfigurationException("Circular configuration variable dependency detected in '{0}'", source);

                    string varValue;
                    // skip unknown variables
                    if (variables.TryGetValue(varName, out varValue))
                    {
                        result = result.Replace(m.Value, varValue);
                        localExpandedVars.Add(varName);
                    }
                }
                // Remember variables expanded during this run. 
                // They should not appear in the next run as that would lead to a circular dependency.
                expandedVars.UnionWith(localExpandedVars.Distinct());
            }

            return result;
        }
    }
}
