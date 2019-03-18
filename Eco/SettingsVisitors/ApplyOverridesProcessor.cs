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
    /// Applies overrides to all matched settings.
    /// Overrides and settings filter is provided by the Eco.applyOverrides configuration element.
    /// </summary>
    public class ApplyOverridesProcessor : TwinSettingsVisitorBase, IFieldValueOverrider
    {
        readonly IReadOnlyDictionary<string, object> _refinedSettingsById;
        readonly IReadOnlyDictionary<object, object> _refinedToRawMap;

        public ApplyOverridesProcessor(
            IReadOnlyDictionary<string, object> refinedSettingsById,
            IReadOnlyDictionary<object, object> refinedToRawMap)
        {
            _refinedSettingsById = refinedSettingsById;
            _refinedToRawMap = refinedToRawMap;
        }

        public event Action<(object settings, string field)> OverridingField;

        public override void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            if (refinedSettings.IsEcoElementOfGenericType(typeof(applyOverrides<>)))
            {
                object rawOverrides = applyOverrides.GetOverrides(rawSettings);
                if (rawOverrides == null) return; // No overrides has been provided.

                Func<FieldInfo, object, bool> IsArrayField = (f, o) => f.FieldType.IsArray;
                var overridenFieldCollector = new OverridenFieldCollector();
                SettingsManager.TraverseSeetingsTree(
                    startNamespace: null,
                    startPath: null,
                    rootMasterSettings: rawOverrides,
                    visitor: overridenFieldCollector, 
                    SkipBranch: IsArrayField);

                object refinedOverrides = applyOverrides.GetOverrides(refinedSettings);
                var targets = applyOverrides.GetTargets(refinedSettings) ??
                    _refinedSettingsById.Keys
                    .Where(k => k.StartsWith(settingsNamespace ?? String.Empty))
                    .Select(k => _refinedSettingsById[k])
                    .Where(s => refinedOverrides.GetType().IsAssignableFrom(s.GetType()));

                foreach (object target in targets)
                {
                    object rawTarget = _refinedToRawMap[target];
                    SettingsManager.TraverseTwinSeetingsTrees(
                        startNamespace: null,
                        startPath: null,
                        rootMasterSettings: rawOverrides,
                        rootSlaveSettings: rawTarget,
                        visitor: new OverridesSetter(overridenFieldCollector.PathsToOverride, notifyOverridingField: OverridingField),
                        SkipBranch: IsArrayField);

                    SettingsManager.TraverseTwinSeetingsTrees(
                        startNamespace: null,
                        startPath: null,
                        rootMasterSettings: refinedOverrides,
                        rootSlaveSettings: target,
                        visitor: new OverridesSetter(overridenFieldCollector.PathsToOverride, notifyOverridingField: null), 
                        SkipBranch: IsArrayField);
                }
            }
        }

        class OverridenFieldCollector : SettingsVisitorBase
        {
            public HashSet<string> PathsToOverride { get; } = new HashSet<string>();

            public override void Visit(string settingsNamespace, string fieldPath, object settings, FieldInfo settingsField)
            {
                var fieldValue = settingsField.GetValue(settings);
                if (settingsField.GetValue(settings) != null && !fieldValue.GetType().IsSettingsType())
                    PathsToOverride.Add(fieldPath);
            }
        }

        class OverridesSetter : TwinSettingsVisitorBase
        {
            readonly HashSet<string> _fieldsToOverride;
            readonly Action<(object settings, string field)> _notifyOverridingField;

            public OverridesSetter(HashSet<string> fieldsToOverride, Action<(object settings, string field)> notifyOverridingField)
            {
                _fieldsToOverride = fieldsToOverride;
                _notifyOverridingField = notifyOverridingField;
            }

            public override void Visit(string settingsNamespace, string fieldPath, object overrides, FieldInfo overridesField, object target, FieldInfo targetField)
            {
                if (targetField.IsDefined<SealedAttribute>()) return;
                if (_fieldsToOverride.Contains(fieldPath))
                {
                    _notifyOverridingField?.Invoke((target, targetField.Name));
                    object overridesValue = overridesField.GetValue(overrides);
                    targetField.SetValue(target, overridesValue);
                }
            }
        }
    }
}
