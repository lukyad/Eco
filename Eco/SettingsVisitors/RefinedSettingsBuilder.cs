using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
    public class RefinedSettingsBuilder : IRefinedSettingsVisitor
    {
        readonly Dictionary<Type, Type> _typeMappings = new Dictionary<Type, Type>();

        public bool IsReversable { get { return true; } }

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
            // if source string is null, return null object.
            if (sourceStr == null) return null;

            object result = null;
            // If field is market with any Converter attributes, go through the converters list and try to parse the string
            // Assign result to the first non-null string.
            ConverterAttribute[] converters = targetField.GetCustomAttributes<ConverterAttribute>().ToArray();
            if (converters != null && converters.Length > 0)
            {
                result = converters
                    .OrderBy(c => c.IsDefault ? 0 : 1) // try default convertors first
                    .Select(c => c.FromString(sourceStr))
                    .FirstOrDefault(o => o != null);
            }
            // If there were no converters or no convertor was able to parse the string,
            // Use native TryParse method. Throw if the method doesn't exist.
            if (result == null)
            {
                Type targetType = targetField.FieldType;
                MethodInfo tryParseMethod = GetTryParseMethod(targetType);
                var args = new object[] { sourceStr, Activator.CreateInstance(targetType) };
                bool parsed = (bool)tryParseMethod.Invoke(null, args);
                if (!parsed) throw new ConfigurationException("Failed to parse '{0}' from '{1}'", targetType.Name, sourceStr);
                result = args[1];
            }
            return result;
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
