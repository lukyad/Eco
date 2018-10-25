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
        readonly Dictionary<string, object> _refinedSettingsById;
        readonly Dictionary<object, object> _refinedToRawMap;
        readonly HashSet<Tuple<object, FieldInfo>> _initializedFields;


        public ApplyDefaultsProcessor(
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

                foreach (object target in targets)
                {
                    var toBeDefaultedFieldCollector = new ToBeDefaultedFieldCollector();
                    object rawTarget = _refinedToRawMap[target];
                    SettingsManager.TraverseSeetingsTree(
                        startNamespace: settingsNamespace,
                        startPath: settingsPath,
                        rootMasterSettings: rawTarget,
                        visitor: toBeDefaultedFieldCollector,
                        SkipBranch: IsArrayField);

                    SettingsManager.TraverseTwinSeetingsTrees(
                        startNamespace: settingsNamespace,
                        startPath: settingsPath,
                        rootMasterSettings: rawDefaults,
                        rootSlaveSettings: rawTarget,
                        visitor: new DefaultsSetter(toBeDefaultedFieldCollector.PathsToDefault, new HashSet<Tuple<object, FieldInfo>>()),
                        SkipBranch: IsArrayField);

                    SettingsManager.TraverseTwinSeetingsTrees(
                        startNamespace: settingsNamespace,
                        startPath: settingsPath,
                        rootMasterSettings: refinedDefaults,
                        rootSlaveSettings: target,
                        visitor: new DefaultsSetter(toBeDefaultedFieldCollector.PathsToDefault, _initializedFields),
                        SkipBranch: IsArrayField);
                }
            }
        }

        // Field path has the following format: containingObjectType.fieldName.nestedFieldName...
        // This function returns fieldPath without the very first part of the path - containingObjectType.
        static string NonRootedFieldPath(string fieldPath)
        {
            return fieldPath.Remove(0, fieldPath.IndexOf('.'));
        }

        class ToBeDefaultedFieldCollector : SettingsVisitorBase
        {
            public HashSet<string> PathsToDefault { get; } = new HashSet<string>();

            public override void Visit(string settingsNamespace, string fieldPath, object settings, FieldInfo settingsField)
            {
                var fieldValue = settingsField.GetValue(settings);
                if (settingsField.GetValue(settings) == null)
                    PathsToDefault.Add(NonRootedFieldPath(fieldPath));
            }
        }

        class DefaultsSetter : TwinSettingsVisitorBase
        {
            readonly HashSet<string> _fieldsToBeDefaulted;
            readonly HashSet<Tuple<object, FieldInfo>> _initializedFields;

            public DefaultsSetter(HashSet<string> fieldsToBeDefaulted,/*out*/ HashSet<Tuple<object, FieldInfo>> initializedFields)
            {
                _fieldsToBeDefaulted = fieldsToBeDefaulted;
                _initializedFields = initializedFields;
            }

            public override void Visit(string settingsNamespace, string fieldPath, object defaults, FieldInfo defaultsField, object target, FieldInfo targetField)
            {
                if (targetField.IsDefined<SealedAttribute>()) return;
                if (!_fieldsToBeDefaulted.Contains(NonRootedFieldPath(fieldPath))) return;

                object defaultValue = defaultsField.GetValue(defaults);
                if (defaultValue != null)
                {
                    targetField.SetValue(target, defaultsField.GetValue(defaults));
                    _initializedFields.Add(Tuple.Create(target, targetField));
                }
            }
        }
    }
}
