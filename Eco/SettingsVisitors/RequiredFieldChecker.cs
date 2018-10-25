using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    public class RequiredFieldChecker : TwinSettingsVisitorBase
    {
        readonly HashSet<Tuple<object, FieldInfo>> _defaultedAndOverridenFields;

        public RequiredFieldChecker(HashSet<Tuple<object, FieldInfo>> defaultedAndOverridenFields)
        {
            _defaultedAndOverridenFields = defaultedAndOverridenFields;
        }

        public override void Visit(string settingsNamesapce, string fieldPath, object refinedSettings, FieldInfo refinedSettingsField, object rawSettings, FieldInfo rawSettingsField)
        {
            if (!rawSettingsField.IsDefined<RequiredAttribute>() || refinedSettingsField.DeclaringType.IsDefined<PrototypeAttribute>()) return;

            bool fieldInitialized =
                rawSettingsField != null && rawSettingsField.GetValue(rawSettings) != null ||
                // Some refined fields could be initialized through applyDefaults or applyOverrides.
                _defaultedAndOverridenFields.Contains(Tuple.Create(refinedSettings, refinedSettingsField));

            if (!fieldInitialized)
                throw new ConfigurationException("Missing required field '{0}'.", fieldPath);
        }
    }
}
