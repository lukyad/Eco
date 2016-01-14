﻿using System;
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
        readonly Dictionary<string, object> _settingsById;

        public ApplyOverridesProcessor(Dictionary<string, object> settingsById)
            : base(isReversable: true)
        {
            _settingsById = settingsById;
        }

        public override void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            if (refinedSettings.IsEcoElementOfGenericType(typeof(applyOverrides<>)))
            {
                Func<FieldInfo, object, bool> IsArrayField = (f, o) => f.FieldType.IsArray;
                var overridenFieldCollector = new OverridenFieldCollector();
                object rawOverrides = applyOverrides.GetOverrides(rawSettings);
                SettingsManager.TraverseSeetingsTree(rawOverrides, overridenFieldCollector, SkipBranch: IsArrayField);

                object refinedOverrides = applyOverrides.GetOverrides(refinedSettings);
                var targets = applyOverrides.GetTargets(refinedSettings) ??
                    _settingsById.Keys
                    .Where(k => k.StartsWith(settingsNamespace ?? String.Empty))
                    .Select(k => _settingsById[k])
                    .Where(s => refinedOverrides.GetType().IsAssignableFrom(s.GetType()));

                foreach (object target in targets)
                    SettingsManager.TraverseTwinSeetingsTrees(refinedOverrides, target, new OverridesSetter(overridenFieldCollector.PathsToOverride), SkipBranch: IsArrayField);
            }
        }

        class OverridenFieldCollector : SettingsVisitorBase
        {
            public HashSet<string> PathsToOverride { get; } = new HashSet<string>();

            public override void Visit(string settingsNamespace, string fieldPath, FieldInfo settingsField, object settings)
            {
                var fieldValue = settingsField.GetValue(settings);
                if (settingsField.GetValue(settings) != null && !fieldValue.GetType().IsSettingsType())
                    PathsToOverride.Add(fieldPath);
            }
        }

        class OverridesSetter : TwinSettingsVisitorBase
        {
            readonly HashSet<string> _fieldsToOverride;

            public OverridesSetter(HashSet<string> fieldsToOverride)
            {
                _fieldsToOverride = fieldsToOverride;
            }

            public override void Visit(string settingsNamespace, string fieldPath, FieldInfo overridesField, object overrides, FieldInfo targetField, object target)
            {
                if (targetField.IsDefined<SealedAttribute>()) return;
                if (_fieldsToOverride.Contains(fieldPath))
                {
                    object overridesValue = overridesField.GetValue(overrides);
                    targetField.SetValue(target, overridesValue);
                }
            }
        }
    }
}