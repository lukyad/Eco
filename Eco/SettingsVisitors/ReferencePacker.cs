using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    public class ReferencePacker : TwinSettingsVisitorBase
    {
        readonly Dictionary<object, string> _namespaceMap;

        public ReferencePacker(Dictionary<object, string> namespaceMap)
        {
            if (namespaceMap == null) throw new ArgumentNullException(nameof(namespaceMap));
            _namespaceMap = namespaceMap;
        }

        public override void Visit(string settingsNamesapce, string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.IsDefined<RefAttribute>())
            {
                if (refinedSettingsField.FieldType.IsSettingsOrObjectType()) PackReference(fieldPath, refinedSettingsField, refinedSettings, rawSettingsField, rawSettings);
                else if (refinedSettingsField.FieldType.IsSettingsOrObjectArrayType()) PackReferenceArray(fieldPath, refinedSettingsField, refinedSettings, rawSettingsField, rawSettings);
                else throw new ConfigurationException("Did not expect to get here.");
            }
        }

        void PackReference(string settingsPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            object settings = refinedSettingsField.GetValue(refinedSettings);
            rawSettingsField.SetValue(rawSettings, GetSettingsWildcard(settings));
        }

        void PackReferenceArray(string settingsPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            Array settingsArray = (Array)refinedSettingsField.GetValue(refinedSettings);
            string references = null;
            if (settingsArray != null)
            {
                var wildcards = new HashSet<string>();
                for (int i = 0; i < settingsArray.Length; i++)
                {
                    var settings = settingsArray.GetValue(i);
                    if (settings != null)
                        wildcards.Add(GetSettingsWildcard(settings));
                }
                references = String.Join(Settings.IdSeparator.ToString(), wildcards.ToArray()).TrimEnd(Settings.IdSeparator);
            }
            rawSettingsField.SetValue(rawSettings, references);
        }

        string GetSettingsWildcard(object settings)
        {
            if (settings == null) return null;
            string id = (string)settings.GetType().GetFields().SingleOrDefault(f => f.IsDefined<IdAttribute>())?.GetValue(settings) ?? Wildcard.Everything;
            string type = settings.GetType().Name;
            return 
                // Namespace combined with id.
                SettingsPath.Combine(_namespaceMap[settings], id) + 
                // Type filter.
                ReferenceResolver.ControlChars.WildcardTypeSeparator + type;
        }
    }
}
