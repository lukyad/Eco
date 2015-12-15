using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    public class SettingsMapBuilder : TwinSettingsVisitorBase
    {
        public Dictionary<string, object> SettingsById { get; } = new Dictionary<string, object>();

       public override void Initialize(Type rootRefinedSettingsType, Type rootRawSettingsType)
        {
            this.SettingsById.Clear();
            this.SettingsById.Add(Settings.NullId, Settings.Null);
        }

        public override void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            string id = GetSettingsId(settingsNamespace, settingsPath, refinedSettings);
            if (id == Settings.NullId) throw new ConfigurationException("'null' settins id is reserved by the Eco library. Please use another id.", id);
            if (this.SettingsById.ContainsKey(id)) throw new ConfigurationException("Duplicate settings ID: '{0}'.", id);
            this.SettingsById.Add(id, refinedSettings);
        }

        string GetSettingsId(string settingsNamesapce, string fieldPath, object refinedSettings)
        {
            string id = null;
            FieldInfo idField = refinedSettings.GetType().GetFields().FirstOrDefault(f => f.IsDefined<IdAttribute>());
            if (idField != null) id = (string)idField.GetValue(refinedSettings);
            if (id == null) id = fieldPath;
            return SettingsPath.Combine(settingsNamesapce, id);
        }
    }
}
