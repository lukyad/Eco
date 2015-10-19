using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco
{
    public class ReferenceResolver : IRefinedSettingsVisitor
    {
        readonly Dictionary<string, object> _settingsById;

        public ReferenceResolver(Dictionary<string, object> settingsById)
        {
            _settingsById = settingsById;
        }

        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootSettingsType) { }

        public void Visit(string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.IsDefined<RefAttribute>())
            {
                if (refinedSettingsField.FieldType.IsArray) ResolveReferenceArray(rawSettingsField, rawSettings, refinedSettingsField, refinedSettings); 
                else ResolveReference(rawSettingsField, rawSettings, refinedSettingsField, refinedSettings);
            }
        }

        void ResolveReference(FieldInfo rawSettingsField, object rawSettings, FieldInfo refinedSettingsField, object refinedSettings)
        {
            string id = (string)rawSettingsField.GetValue(rawSettings);
            if (id != null)
            {
                object settings = GetSettings(id, throwIfMissing: !refinedSettingsField.GetCustomAttribute<RefAttribute>().Weak);
                if (settings != null && !refinedSettingsField.FieldType.IsAssignableFrom(settings.GetType()))
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
                Func<string, IEnumerable<string>> MatchIds = w => _settingsById.Keys.Where(id => new Wildcard(w.Trim()).IsMatch(id));
                var elementType = refinedSettingsField.FieldType.GetElementType();
                var settingsList = new List<object>();
                foreach (var wildcard in idWildcards.Split(','))
                {
                    var matchedIds = _settingsById.Keys.Where(id => new Wildcard(wildcard.Trim()).IsMatch(id)).ToArray();
                    if (matchedIds.Length == 0 && !refinedSettingsField.GetCustomAttribute<RefAttribute>().Weak)
                        throw new ConfigurationException("Could not find any settings matching '{0}' id", wildcard);

                    for (int i = 0; i < matchedIds.Length; i++)
                    {
                        object settings = GetSettings(matchedIds[i]);
                        if (settings != null)

                        {
                            if (!elementType.IsAssignableFrom(settings.GetType()))
                            {
                                throw new ConfigurationException("Could not assign object with ID='{0}' of type '{1}' to an element of the array '{2}.{3}' of type '{4}'",
                                    matchedIds[i], settings.GetType().Name, refinedSettingsField.DeclaringType.Name, refinedSettingsField.Name, elementType.Name);
                            }
                            settingsList.Add(settings);
                        }
                    }
                }
                Array settingsArray = Array.CreateInstance(elementType, settingsList.Count);
                for (int i = 0; i < settingsList.Count; i++)
                    settingsArray.SetValue(settingsList[i], i);

                refinedSettingsField.SetValue(refinedSettings, settingsArray);
            }
        }

        object GetSettings(string id, bool throwIfMissing = false)
        {
            object settings;
            if (!_settingsById.TryGetValue(id, out settings) && throwIfMissing) throw new ConfigurationException("Missing configuration ID: {0}", id);
            return settings;
        }

    }
}
