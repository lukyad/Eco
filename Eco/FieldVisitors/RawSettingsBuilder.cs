using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
    class RawSettingsBuilder : IFieldVisitor
    {
        readonly Dictionary<Type, Type> _typeMappings = new Dictionary<Type, Type>();

        public void Visit(string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.IsDefined<RefAttribute>()) return;

            object rawValue = null;
            object refinedValue = refinedSettingsField.GetValue(refinedSettings);
            if (refinedValue != null)
            {
                Type refinedValueType = refinedValue.GetType();
                if (refinedValueType.IsSettingsType())
                {
                    rawValue = SettingsConstruction.CreateSettingsObject(refinedValue, rawSettingsField, _typeMappings);
                }
                else if (refinedValueType.IsSettingsArrayType())
                {
                    rawValue = SettingsConstruction.CreateSettingsArray((Array)refinedValue, rawSettingsField, _typeMappings);
                }
                else if (refinedValueType != typeof(string) && rawSettingsField.FieldType == typeof(string))
                {
                    rawValue = ToString(refinedSettingsField, refinedSettings);
                }
                else
                {
                    rawValue = refinedSettingsField.GetValue(refinedSettings);
                }

                if (refinedSettingsField.IsDefined<FieldMutatorAttribute>())
                {
                    refinedSettingsField.GetCustomAttribute<FieldMutatorAttribute>().SetRawSettingsFieldValue(rawSettingsField, rawSettings, rawValue);
                }
                else
                {
                    rawSettingsField.SetValue(rawSettings, rawValue);
                }
            }
        }

        static string ToString(FieldInfo sourceField, object container)
        {
            object value = sourceField.GetValue(container);
            if (value != null && Nullable.GetUnderlyingType(sourceField.FieldType) != null)
                value = sourceField.FieldType.GetProperty("Value").GetValue(value);

            ConverterAttribute converter = sourceField.GetCustomAttribute<ConverterAttribute>();
            if (converter != null)
                return converter.ToString(value);
            else
                return value != null ? value.ToString() : null;
        }
    }
}
