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
    public class ApplyDefaultsProcessor : TwinSettingsVisitorBase, IDefaultValueSetter
    {
        readonly IReadOnlyDictionary<string, object> _refinedSettingsById;
        readonly IReadOnlyDictionary<object, object> _refinedToRawMap;


        public ApplyDefaultsProcessor(
            IReadOnlyDictionary<string, object> refinedSettingsById,
            IReadOnlyDictionary<object, object> refinedToRawMap)
        {
            _refinedSettingsById = refinedSettingsById;
            _refinedToRawMap = refinedToRawMap;
        }

        public event Action<(object settings, string field)> InitializingField;

        public override void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            if (refinedSettings.IsEcoElementOfGenericType(typeof(applyDefaults<>)))
            {
                Func<FieldInfo, object, bool> IsArrayField = (f, o) => f.FieldType.IsArray;

                object refinedDefaults = applyDefaults.GetDefaults(refinedSettings);
                object rawDefaults = applyDefaults.GetDefaults(rawSettings);
                var targets = applyDefaults.GetTargets(refinedSettings) ??
                    _refinedSettingsById.Keys
                    .Where(k => k.StartsWith(settingsNamespace ?? String.Empty))
                    .Select(k => _refinedSettingsById[k])
                    .Where(s => refinedDefaults.GetType().IsAssignableFrom(s.GetType()));

                var toBeDefaultedFieldCollector = new ToBeDefaultedFieldCollector();
                SettingsManager.TraverseSeetingsTree(
                    startNamespace: null,
                    startPath: null,
                    rootMasterSettings: rawDefaults,
                    visitor: toBeDefaultedFieldCollector,
                    SkipBranch: IsArrayField);

                foreach (object target in targets)
                {
                    object rawTarget = _refinedToRawMap[target];
                    SettingsManager.TraverseTwinSeetingsTrees(
                        startNamespace: null,
                        startPath: null,
                        rootMasterSettings: rawDefaults,
                        rootSlaveSettings: rawTarget,
                        visitor: new DefaultsSetter(toBeDefaultedFieldCollector.PathsToDefault, notifyInitializingField: InitializingField),
                        SkipBranch: IsArrayField);

                    SettingsManager.TraverseTwinSeetingsTrees(
                        startNamespace: null,
                        startPath: null,
                        rootMasterSettings: refinedDefaults,
                        rootSlaveSettings: target,
                        visitor: new DefaultsSetter(toBeDefaultedFieldCollector.PathsToDefault, notifyInitializingField: null),
                        SkipBranch: IsArrayField);
                }
            }
        }

        class ToBeDefaultedFieldCollector : SettingsVisitorBase
        {
            public HashSet<string> PathsToDefault { get; } = new HashSet<string>();

            public override void Visit(string settingsNamespace, string fieldPath, object settings, FieldInfo settingsField)
            {
                var fieldValue = settingsField.GetValue(settings);
                if (settingsField.GetValue(settings) != null)
                    PathsToDefault.Add(fieldPath);
            }
        }

        class DefaultsSetter : TwinSettingsVisitorBase
        {
            readonly HashSet<string> _fieldsToBeDefaulted;
            readonly Action<(object settings, string field)> _notifyInitializingField;

            public DefaultsSetter(HashSet<string> fieldsToBeDefaulted, Action<(object settings, string field)> notifyInitializingField)
            {
                _fieldsToBeDefaulted = fieldsToBeDefaulted;
                _notifyInitializingField = notifyInitializingField;
            }

            public override void Visit(string settingsNamespace, string fieldPath, object defaults, FieldInfo defaultsField, object target, FieldInfo targetField)
            {
                if (targetField.IsDefined<SealedAttribute>()) return;
                if (!_fieldsToBeDefaulted.Contains(fieldPath)) return;

                if (fieldPath == "async") { }
                _notifyInitializingField?.Invoke((target, targetField.Name));
                object targetValue = defaultsField.GetValue(target);
                if (targetValue == null)
                    targetField.SetValue(target, defaultsField.GetValue(defaults));
            }
        }
    }
}
