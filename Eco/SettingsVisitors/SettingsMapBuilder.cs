using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    public class SettingsMapBuilder : TwinSettingsVisitorBase, IDynamicSettingsConstructorObserver
    {
        readonly Dictionary<string, object> _refinedSettingsById = new Dictionary<string, object>();
        readonly Dictionary<object, object>  _refinedToRawMap = new Dictionary<object, object>();

        public IReadOnlyDictionary<string, object> RefinedSettingsById => _refinedSettingsById;

        public IReadOnlyDictionary<object, object> RefinedToRawMap => _refinedToRawMap;

        public override void Initialize(Type rootRefinedSettingsType, Type rootRawSettingsType)
        {
            this._refinedSettingsById.Clear();
            this._refinedToRawMap.Clear();
            this._refinedSettingsById.Add(Settings.NullId, Settings.Null);
        }

        public override void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            string id = GetSettingsId(settingsNamespace, settingsPath, refinedSettings);
            if (id == Settings.NullId) throw new ConfigurationException("'null' settins id is reserved by the Eco library. Please use another id.", id);
            if (this.RefinedSettingsById.ContainsKey(id)) throw new ConfigurationException("Duplicate settings ID: '{0}'.", id);
            _refinedSettingsById.Add(id, refinedSettings);
            _refinedToRawMap.Add(refinedSettings, rawSettings);
        }

        string GetSettingsId(string settingsNamesapce, string fieldPath, object refinedSettings)
        {
            string id = null;
            FieldInfo idField = refinedSettings.GetType().GetFields().FirstOrDefault(f => f.IsDefined<IdAttribute>());
            if (idField != null) id = (string)idField.GetValue(refinedSettings);
            if (id == null) id = fieldPath;
            return SettingsPath.Combine(settingsNamesapce, id);
        }

        public void Observe(IDynamicSettingsConstructor ctor)
        {
            ctor.SettingsCreated += s =>
            {
                _refinedSettingsById.Add(s.settingsId, s.refinedSettings);
                _refinedToRawMap.Add(s.refinedSettings, s.rawSettings);
            };
        }
    }
}
