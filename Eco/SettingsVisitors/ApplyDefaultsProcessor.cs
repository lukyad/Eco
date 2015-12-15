using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Applies defaults to all matched settings.
    /// Defaults and settings filter is provided by the Eco.applyDefaults configuration element.
    /// </summary>
    public class ApplyDefaultsProcessor : TwinSettingsVisitorBase
    {
        readonly Dictionary<string, object> _settingsById;

        public ApplyDefaultsProcessor(Dictionary<string, object> settingsById)
        {
            _settingsById = settingsById;
        }

        public override void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            if (refinedSettings.IsEcoElementOfGenericType(typeof(applyDefaults<>)))
            {
                object defaults = applyDefaults.GetDefaults(refinedSettings);
                var targets = applyDefaults.GetTargets(refinedSettings) ??
                    _settingsById.Keys
                    .Where(k => k.StartsWith(settingsNamespace ?? String.Empty))
                    .Select(k => _settingsById[k])
                    .Where(s => defaults.GetType().IsAssignableFrom(s.GetType()));
                foreach (object target in targets)
                    SettingsManager.TraverseTwinSeetingsTrees(defaults, target, new DefaultsSetter(), SkipBranch: (f, o) => f.FieldType.IsArray);
            }
        }

        class DefaultsSetter : TwinSettingsVisitorBase
        {
            public override void Visit(string settingsNamespace, string fieldPath, FieldInfo defaultsField, object defaults, FieldInfo targetField, object target)
            {
                if (targetField.IsDefined<SealedAttribute>()) return;
                object targetValue = targetField.GetValue(target);
                object defaultValue = defaultsField.GetValue(defaults);
                bool isFinalPath = targetField.IsDefined<RefAttribute>() || defaultValue == null || !defaultValue.GetType().IsSettingsType();
                if (targetValue == null && defaultValue != null && isFinalPath)
                    targetField.SetValue(target, defaultsField.GetValue(defaults));
            }
        }
    }
}
