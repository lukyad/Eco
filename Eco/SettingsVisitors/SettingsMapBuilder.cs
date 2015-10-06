using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
    class SettingsMapBuilder : IRefinedSettingsVisitor
    {
        readonly Dictionary<string, object> _settingsById = new Dictionary<string, object>();

        public bool IsReversable { get { return true; } }

        public Dictionary<string, object> SettingsById{ get { return _settingsById; } }

        public void Initialize(Type rootSettingsType)
        {
            _settingsById.Clear();
        }

        public void Visit(string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.IsDefined<IdAttribute>())
            {
                string id = (string)rawSettingsField.GetValue(rawSettings);
                if (_settingsById.ContainsKey(id)) throw new ConfigurationException("Duplicated settings ID: '{0}'", id);
                _settingsById.Add(id, refinedSettings);
            }
        }
    }
}
