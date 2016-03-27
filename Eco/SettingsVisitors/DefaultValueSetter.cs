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
    /// Used by the Eco library to set default raw setting values.
    /// </summary>
    public class DefaultValueSetter : ISettingsVisitor
    {
        // Changes made by the DefaultValueSetter are not revocable.
        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRawSettingsType) { }

        public void Visit(string settingsNamespace, string fieldPath, object rawSettings) { }

        public void Visit(string settingsNamespace, string fieldPath, FieldInfo rawSettingsField, object rawSettings)
        {
            var defaultAttr = rawSettingsField.GetCustomAttribute<DefaultAttribute>();
            if (defaultAttr != null)
            {
                if (!CanAssign(rawSettingsField, defaultAttr.Value))
                    throw new ConfigurationException("Invalid default field value for {0}.{1}: '{2}'.", rawSettingsField.DeclaringType.Name, rawSettingsField.Name, defaultAttr.Value);

                object targetValue = rawSettingsField.GetValue(rawSettings);
                if (targetValue == null)
                {
                    if (rawSettingsField.FieldType == typeof(string)) rawSettingsField.SetValue(rawSettings, defaultAttr.Value.ToString());
                    else rawSettingsField.SetValue(rawSettings, defaultAttr.Value);
                }
            }
        }

        static bool CanAssign(FieldInfo rawSettingsField, object value)
        {
            // Any value can be (and will be, if needed) converted to string.
            if (rawSettingsField.FieldType == typeof(string)) return true;
            // null can be assigned only to a field of a reference type.
            if (value == null) return !rawSettingsField.FieldType.IsValueType;
            // Here is a bit of magic for Nullble types.
            return rawSettingsField.FieldType.IsNullable() && !value.GetType().IsNullable() ?
                Nullable.GetUnderlyingType(rawSettingsField.FieldType).IsAssignableFrom(value.GetType()) :
                rawSettingsField.FieldType.IsAssignableFrom(value.GetType());

        }
    }
}
