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
    /// Used by the Eco library to expand referenced fileds in the configuration file.
    /// Any field of a settings class can reference any other field of any type defined in the same class using the following syntax: @{filedName}
    /// 
    /// Example:
    /// 
    /// class foo
    /// {
    ///     public string fieldA = "123"
    ///     public int fieldB = 4;
    ///     public string fieldC = "abc{fieldA}{fieldC}edf";
    /// }
    /// 
    /// The final value of fieldB will be "abs1234edf";
    /// 
    /// Field reference expansion is performed on the 'raw settings' (not the refined one),
    /// this allows using of field references in any raw settings field of the string type including the ones marked with the Ref attribute.
    /// 
    /// If Eco detects a reference to a non-exiting field, it just doesn't expand the reference and leave it as is. 
    /// </summary>
    public class FieldReferenceExpander : TwinSettingsVisitorBase
    {
        // Changes made by the StringFieldReferenceExpander are not revocable.
        // i.e. it's not possible to pack expanded strings back.
        public FieldReferenceExpander()
            : base(isReversable: false)
        {
        }

        public override void Visit(string settingsNamesapce, string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (rawSettingsField == null)
                return;

            // Skip 'sealed' fields.
            if (rawSettingsField.IsDefined<SealedAttribute>()) return;

            if (rawSettingsField.FieldType == typeof(string))
            {
                var originalString = (string)rawSettingsField.GetValue(rawSettings);
                if (originalString != null)
                {
                    var expandedString = ExpandFieldReferences(originalString, fieldPath, rawSettings);
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
                        var expandedString = ExpandFieldReferences(originalString, fieldPath, rawSettings);
                        arr[i] = expandedString;
                    }
                }
            }
        }

        static string ExpandFieldReferences(string sourceValue, string sourcePath, object rawSettings)
        {
            string result = sourceValue;
            var expandedVars = new HashSet<string>();
            // Match all fields referenced in the current string.
            var regex = new Regex(@"\@\{(?<fieldName>[\w\.]+)\}");
            MatchCollection fieldMatches = regex.Matches(result);
            if (fieldMatches.Count > 0)
            {
                // Expand matched fields.
                foreach (Match m in fieldMatches)
                {
                    // Field name.
                    string fieldName = m.Groups["fieldName"].Value;
                    // Make sure that rawSettings contains referenced field.
                    var referencedFiled = rawSettings.GetType().GetField(fieldName);
                    if (referencedFiled == null)
                        throw new ConfigurationException("Settings type '{0}' doesn't contain filed '{1}' referenced by '{2}'.", rawSettings.GetType(), fieldName, sourcePath);

                    // Make sure that matched field value doesn't not contain references to other fields.
                    string referencedFieldValue = rawSettings.GetFieldValue(fieldName)?.ToString();
                    if (referencedFieldValue == null)
                        continue;

                    if (regex.Matches(referencedFieldValue).Count > 0)
                        throw new ConfigurationException("'{0}' references the field '{1}' which also contains a field reference. Double dereferencing is not supported.", sourcePath, fieldName);

                    result = result.Replace(m.Value, referencedFieldValue);
                }
            }

            return result;
        }
    }
}
