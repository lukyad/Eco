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
    /// Applies ref list modifications to all matched settings.
    /// Modifications and settings filter is provided by the Eco.applyOverrides configuration element.
    /// </summary>
    public class RefListModificationProcessor : TwinSettingsVisitorBase
    {
        readonly IReadOnlyDictionary<string, object> _refinedSettingsById;

        public RefListModificationProcessor(IReadOnlyDictionary<string, object> refinedSettingsById)
        {
            _refinedSettingsById = refinedSettingsById;
        }

        public override void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            if (refinedSettings.IsEcoElementOfGenericType(typeof(applyOverrides<>)))
            {
                Type overridesType = applyOverrides.GetOverridesType(refinedSettings);
                var targets = applyOverrides.GetTargets(refinedSettings) ??
                    _refinedSettingsById.Keys
                    .Where(k => k.StartsWith(settingsNamespace ?? String.Empty))
                    .Select(k => _refinedSettingsById[k])
                    .Where(s => overridesType.IsAssignableFrom(s.GetType()));

                foreach (object target in targets)
                {
                    // Below we apply all ref list modifications (if any)
                    // All modifications get applied to the refined settings only.
                    foreach (var spec in applyOverrides.GetRefListModifications(refinedSettings))
                        ApplyRefListModification(target, spec);
                }

                void ApplyRefListModification(object refinedTarget, modifyRefList spec)
                {
                    var targetField = overridesType.GetField(spec.field);
                    if (targetField == null || !targetField.FieldType.IsArray || !targetField.IsDefined<RefAttribute>())
                        throw new ConfigurationException("Settings of type '{0}' doesn't contain field of a ref list type with name '{1}'.", overridesType, spec.field);

                    object[] list = (object[])targetField.GetValue(refinedTarget);
                    foreach (var cmd in spec.commands)
                    {
                        try
                        {
                            list = cmd.Apply(list);
                        }
                        catch (Exception e)
                        {
                            throw new ConfigurationException(e, "Failed to modify reference list: {0}", settingsPath);
                        }
                    }
                    refinedTarget.SetFieldValue(spec.field, list);
                }
            }
        }
    }
}
