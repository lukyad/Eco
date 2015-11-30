using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    public class SettingsMapBuilder : IRefinedSettingsVisitor
    {
        readonly Dictionary<string, object> _settingsById = new Dictionary<string, object>();
        readonly SortedList<string, string> _namespaces = new SortedList<string, string>();

        public bool IsReversable { get { return true; } }

        public Dictionary<string, object> SettingsById{ get { return _settingsById; } }

        public void Initialize(Type rootSettingsType)
        {
            _settingsById.Clear();
            _namespaces.Clear();
        }

        public void Visit(string fieldPath, string fieldNamesapce, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.IsDefined<IdAttribute>())
            {
                string id = (string)rawSettingsField.GetValue(rawSettings);
                if (id != null)
                {
                    string fullId = SettingsPath.Combine(fieldNamesapce, id);
                    if (_settingsById.ContainsKey(fullId)) throw new ConfigurationException("Duplicate settings ID: '{0}'.", id);
                    _settingsById.Add(fullId, refinedSettings);
                }
            }
        }
    }
}
