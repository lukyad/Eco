using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
    public class RefinedSettingsBuilder : IRefinedSettingsVisitor
    {
        readonly Dictionary<Type, Type> _typeMappings = new Dictionary<Type, Type>();
        ParsingRuleAttribute[] _parsingRules;

        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootSettingsType)
        {
            // Capture parsing rules that applies to all fields.
            _parsingRules = rootSettingsType.GetCustomAttributes<ParsingRuleAttribute>().ToArray();
        }

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
                else if (rawValueType.IsSettingsOrObjectArrayType())
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

        object FromString(string sourceStr, FieldInfo targetField)
        {
            // if source string is null, return null object.
            if (sourceStr == null) return null;

            object result = null;
            // If field is marked with a Converter attribute, use the Converter.FromString method
            // to get field's value.
            var converter = targetField.GetCustomAttribute<ConverterAttribute>();
            if (converter != null)
            {
                result = converter.FromString(sourceStr);
            }
            else
            {
                // If field is marked with any Parser attributes, go through the parsers list and try to parse the source string.
                // Assign result to the first non-null object.
                result =
                    targetField.GetCustomAttributes<ParserAttribute>()
                    .Select(a => a.Parse(sourceStr, a.Format))
                    .FirstOrDefault(r => r != null);

                // If result is still null, try to use parsingRules.
                if (result == null)
                {
                    result = _parsingRules
                        .Where(r => r.SourceType == targetField.FieldType)
                        .Select(r => r.Parse(sourceStr, r.Format))
                        .FirstOrDefault(o => o != null);
                }
                // If there were no parsers or they have not been able to parse the string,
                // Use native TryParse method. Throw if the method doesn't exist.
                if (result == null)
                {
                    Type targetType = targetField.FieldType;
                    MethodInfo tryParseMethod;
                    Type underlyingNullableType = Nullable.GetUnderlyingType(targetType);
                    if (targetType.IsEnum) tryParseMethod = GetEnumTryParseMethod(targetType);
                    else if (underlyingNullableType != null && underlyingNullableType.IsEnum) tryParseMethod = GetEnumTryParseMethod(underlyingNullableType);
                    else tryParseMethod = GetNativeTryParseMethod(targetType);

                    if (tryParseMethod == null) throw new ConfigurationException("Not able to parse {0}.{1}", targetField.DeclaringType.Name, targetField.FieldType);
                    var args = new object[] { sourceStr, Activator.CreateInstance(targetType) };
                    bool parsed = (bool)tryParseMethod.Invoke(null, args);
                    if (!parsed) throw new ConfigurationException("Failed to parse '{0}' from '{1}'", targetType.Name, sourceStr);
                    result = args[1];
                }
                
            }

            return result;
        }

        static MethodInfo GetNativeTryParseMethod(Type container)
        {
            // TODO validate TryParse method
            const string TryParseMethodName = "TryParse";
            Type valueType = Nullable.GetUnderlyingType(container) ?? container;
            return valueType.GetMethod(TryParseMethodName, new[] { typeof(string), valueType.MakeByRefType() });
        }

        static MethodInfo GetEnumTryParseMethod(Type enumType)
        {
            MethodInfo tryParseMethod = typeof(Enum).GetMethods().First(m => m.Name == "TryParse" && m.GetParameters().Count() == 2);
            return tryParseMethod.MakeGenericMethod(enumType);
        }
    }
}
