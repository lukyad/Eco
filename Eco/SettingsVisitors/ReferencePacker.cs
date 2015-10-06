using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco
{
    public class ReferencePacker : IRefinedSettingsVisitor
    {
        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootSettingsType) { }

        public void Visit(string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.IsDefined<RefAttribute>())
            {
                if (refinedSettingsField.FieldType.IsSettingsType()) PackReference(fieldPath, refinedSettingsField, refinedSettings, rawSettingsField, rawSettings);
                else if (refinedSettingsField.FieldType.IsSettingsArrayType()) PackReferenceArray(fieldPath, refinedSettingsField, refinedSettings, rawSettingsField, rawSettings);
                else throw new ConfigurationException("Did not expect to get here");
            }
        }

        static void PackReference(string settingsPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            object settings = refinedSettingsField.GetValue(refinedSettings);
            if (settings == null) return;
            rawSettingsField.SetValue(rawSettings, GetSettingsId(settingsPath, settings));
        }

        static void PackReferenceArray(string settingsPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            Array settingsArray = (Array)refinedSettingsField.GetValue(refinedSettings);
            if (settingsArray == null) return;
            var referenceListBuilder = new StringBuilder();
            for (int i = 0; i < settingsArray.Length; i++)
            {
                var settings = settingsArray.GetValue(i);
                if (settings != null)
                {
                    string id = GetSettingsId(settingsPath, settings);
                    referenceListBuilder.Append(id + Settings.IdSeparator);
                }
            }
            string referenceList = referenceListBuilder.ToString().TrimEnd(Settings.IdSeparator);
            rawSettingsField.SetValue(rawSettings, String.IsNullOrEmpty(referenceList) ? null : referenceList);
        }

        static string GetSettingsId(string settingsPath, object settings)
        {
            FieldInfo idField = settings.GetType().GetFields().SingleOrDefault(f => f.IsDefined<IdAttribute>());
            if (idField == null)
                throw new ConfigurationException("Expected an object with one of the fields marked with {0}, but got an instance of type {1}", typeof(IdAttribute).Name, settings.GetType().Name);

            string id = (string)idField.GetValue(settings);
            if (id == null) throw new ConfigurationException("Detected null object ID: path={0}", settingsPath);

            return id;
        }
    }
}
