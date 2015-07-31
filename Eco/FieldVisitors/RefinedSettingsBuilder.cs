using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
    class RefinedSettingsBuilder : IFieldVisitor
    {
        readonly Dictionary<Type, Type> _typeMappings = new Dictionary<Type, Type>();

        public void Visit(string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.IsDefined<RefAttribute>()) return;

            object rawValue = refinedSettingsField.IsDefined<FieldMutatorAttribute>() ?
                refinedSettingsField.GetCustomAttribute<FieldMutatorAttribute>().GetRawSettingsFieldValue(rawSettingsField, rawSettings) :
                rawSettingsField.GetValue(rawSettings);

            object refinedValue = null;
            if (rawValue != null)
            {
                var rawValueType = rawValue.GetType();
                if (rawValueType.IsSettingsType())
                {
                    refinedValue = SettingsConstruction.CreateSettingsObject(rawValue, refinedSettingsField, _typeMappings);
                }
                else if (rawValueType.IsSettingsArrayType())
                {
                    refinedValue = SettingsConstruction.CreateSettingsArray((Array)rawValue, refinedSettingsField, _typeMappings);
                }
                else if (rawValueType == typeof(string) && refinedSettingsField.FieldType != typeof(string))
                {
                    refinedValue = FromString((string)rawSettingsField.GetValue(rawSettings), refinedSettingsField);
                }
                else
                {
                    refinedValue = rawSettingsField.GetValue(rawSettings);
                }
            }
            refinedSettingsField.SetValue(refinedSettings, refinedValue);
        }

        static object FromString(string sourceStr, FieldInfo targetField)
        {
            if (sourceStr == null) return null;

            ConverterAttribute converter = targetField.GetCustomAttribute<ConverterAttribute>();
            if (converter != null)
            {
                return converter.FromString(sourceStr);
            }
            else
            {
                Type targetType = targetField.FieldType;
                MethodInfo tryParseMethod = GetTryParseMethod(targetType);
                var args = new object[] { sourceStr, Activator.CreateInstance(targetType) };
                bool parsed = (bool)tryParseMethod.Invoke(null, args);
                if (!parsed) throw new ConfigurationException("Failed to parse '{0}' from '{1}'", targetType.Name, sourceStr);
                return args[1];
            }
        }

        static MethodInfo GetTryParseMethod(Type container)
        {
            // TODO validate TryParse method
            const string TryParseMethodName = "TryParse";
            Type valueType = Nullable.GetUnderlyingType(container) ?? container;
            return valueType.GetMethod(TryParseMethodName, new[] { typeof(string), valueType.MakeByRefType() });
        }
    }
}
