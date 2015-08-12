using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco
{
    class ReferenceResolver : IRefinedSettingsVisitor
    {
        readonly Dictionary<string, object> _settingsById;

        public ReferenceResolver(Dictionary<string, object> settingsById)
        {
            _settingsById = settingsById;
        }

        public bool IsReversable { get { return true; } }

        public void Visit(string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.IsDefined<RefAttribute>())
            {
                if (refinedSettingsField.FieldType.IsSettingsType()) ResolveReference(rawSettingsField, rawSettings, refinedSettingsField, refinedSettings);
                else if (refinedSettingsField.FieldType.IsSettingsArrayType()) ResolveReferenceArray(rawSettingsField, rawSettings, refinedSettingsField, refinedSettings);
                else throw new ConfigurationException("Did not expect to get here");
            }
        }

        void ResolveReference(FieldInfo rawSettingsField, object rawSettings, FieldInfo refinedSettingsField, object refinedSettings)
        {
            string id = (string)rawSettingsField.GetValue(rawSettings);
            if (id != null)
            {
                object settings = GetSettings(id);
                if (!refinedSettingsField.FieldType.IsAssignableFrom(settings.GetType()))
                {
                    throw new ConfigurationException("Could not assign object with ID='{0}' of type '{1}' to the '{2}' field of type '{3}'",
                        id, settings.GetType().Name, refinedSettingsField.Name, refinedSettingsField.FieldType.Name);
                }
                refinedSettingsField.SetValue(refinedSettings, settings);
            }
        }

        void ResolveReferenceArray(FieldInfo rawSettingsField, object rawSettings, FieldInfo refinedSettingsField, object refinedSettings)
        {
            string idWildcards = (string)rawSettingsField.GetValue(rawSettings);
            if (idWildcards != null)
            {
                Func<string, IEnumerable<string>> MatchIds = w => _settingsById.Keys.Where(id => new Wildcard(w).IsMatch(id));
                var ids = idWildcards.Split(',').SelectMany(w => MatchIds(w)).ToArray();
                var elementType = refinedSettingsField.FieldType.GetElementType();
                Array settingsArray = Array.CreateInstance(elementType, ids.Length);
                for (int i = 0; i < ids.Length; i++)
                {
                    object settings = GetSettings(ids[i]);
                    if (!elementType.IsAssignableFrom(settings.GetType()))
                    {
                        throw new ConfigurationException("Could not assign object with ID='{0}' of type '{1}' to an element of the array '{2}' of type '{3}'",
                            ids[i], settings.GetType().Name, refinedSettingsField.Name, elementType.Name);
                    }
                    settingsArray.SetValue(settings, i);
                }
                refinedSettingsField.SetValue(refinedSettings, settingsArray);
            }
        }

        object GetSettings(string id)
        {
            object settings;
            if (!_settingsById.TryGetValue(id, out settings)) throw new ConfigurationException("Missing configuration ID: {0}", id);
            return settings;
        }

    }
}
