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
    public class ApplyOverridesProcessor : TwinSettingsVisitorBase
    {
        readonly Dictionary<string, object> _refinedSettingsById;
        readonly Dictionary<object, object> _refinedToRawMap;
        readonly HashSet<Tuple<object, FieldInfo>> _initializedFields;

        public ApplyOverridesProcessor(
            Dictionary<string, object> refinedSettingsById,
            Dictionary<object, object> refinedToRawMap,
            /*out*/ HashSet<Tuple<object, FieldInfo>> initializedFields)
        {
            _refinedSettingsById = refinedSettingsById;
            _refinedToRawMap = refinedToRawMap;
            _initializedFields = initializedFields;
        }

        public override void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            if (refinedSettings.IsEcoElementOfGenericType(typeof(applyOverrides<>)))
            {
                Func<FieldInfo, object, bool> IsArrayField = (f, o) => f.FieldType.IsArray;
                var overridenFieldCollector = new OverridenFieldCollector();
                object rawOverrides = applyOverrides.GetOverrides(rawSettings);
                SettingsManager.TraverseSeetingsTree(
                    startNamespace: settingsNamespace,
                    startPath: settingsPath,
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
                        startNamespace: settingsNamespace,
                        startPath: settingsPath,
                        rootMasterSettings: rawOverrides,
                        rootSlaveSettings: rawTarget,
                        visitor: new OverridesSetter(overridenFieldCollector.PathsToOverride, new HashSet<Tuple<object, FieldInfo>>()),
                        SkipBranch: IsArrayField);

                    SettingsManager.TraverseTwinSeetingsTrees(
                        startNamespace: settingsNamespace,
                        startPath: settingsPath,
                        rootMasterSettings: refinedOverrides,
                        rootSlaveSettings: target,
                        visitor: new OverridesSetter(overridenFieldCollector.PathsToOverride, _initializedFields), 
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
            readonly HashSet<Tuple<object, FieldInfo>> _initializedFields;

            public OverridesSetter(HashSet<string> fieldsToOverride, /*out*/ HashSet<Tuple<object, FieldInfo>> initializedFields)
            {
                _fieldsToOverride = fieldsToOverride;
                _initializedFields = initializedFields;
            }

            public override void Visit(string settingsNamespace, string fieldPath, object overrides, FieldInfo overridesField, object target, FieldInfo targetField)
            {
                if (targetField.IsDefined<SealedAttribute>()) return;
                if (_fieldsToOverride.Contains(fieldPath))
                {
                    object overridesValue = overridesField.GetValue(overrides);
                    targetField.SetValue(target, overridesValue);
                    _initializedFields.Add(Tuple.Create(target, targetField));
                }
            }
        }
    }
}
