using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco
{
    class SettingsObjectBuilder : IFieldVisitor
    {
        readonly Dictionary<Type, Type> _typeMappings = new Dictionary<Type, Type>();

        public void Visit(string fieldPath, FieldInfo sourceField, object sourceSettings, FieldInfo targetField, object targetSettings)
        {
            if (sourceField.IsDefined<RefAttribute>()) return;

            object targetValue;
            if (sourceField.FieldType.IsSettingsType())
            {
                targetValue = this.CreateSettingsObject(sourceField, sourceSettings, targetField);
            }
            else if (sourceField.FieldType.IsSettingsArrayType())
            {
                targetValue = this.CreateSettingsArray(sourceField, sourceSettings, targetField);
            }
            else if (sourceField.FieldType != typeof(string) && targetField.FieldType == typeof(string))
            {
                targetValue = ToString(sourceField, sourceSettings);
            }
            else if (sourceField.FieldType == typeof(string) && targetField.FieldType != typeof(string))
            {
                targetValue = FromString((string)sourceField.GetValue(sourceSettings), targetField);
            }
            else
            {
                targetValue = sourceField.GetValue(sourceSettings);
            }

            targetField.SetValue(targetSettings, targetValue);
        }

        object CreateSettingsObject(FieldInfo sourceField, object sourceSettings, FieldInfo targetField)
        {
            object sourceObject = sourceField.GetValue(sourceSettings);
            if (sourceObject == null) return null;

            Type sourceObjectType = sourceObject.GetType();
            Type targetObjectType = GetTargetType(sourceObjectType, targetField.FieldType.Assembly);
            return Activator.CreateInstance(targetObjectType);
        }

        object CreateSettingsArray(FieldInfo sourceField, object sourceSettings, FieldInfo targetField)
        {
            Array sourceArray = (Array)sourceField.GetValue(sourceSettings);
            if (sourceArray == null) return null;

            int sourceArrayLength = sourceArray.Length;
            Array targetArray = Array.CreateInstance(targetField.FieldType.GetElementType(), sourceArrayLength);
            for (int i = 0; i < targetArray.Length; i++)
            {
                Type sourceElementType = sourceArray.GetValue(i).GetType();
                Type targetElementType = GetTargetType(sourceElementType, targetField.FieldType.Assembly);
                targetArray.SetValue(Activator.CreateInstance(targetElementType), i);
            }
            return targetArray;
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

        Type GetTargetType(Type sourceType, Assembly targetAssembly)
        {
            Type targetType;
            if (!_typeMappings.TryGetValue(sourceType, out targetType))
            {
                targetType = targetAssembly.GetTypes().FirstOrDefault(t => t.Name == sourceType.Name);
                if (targetType == null) throw new ConfigurationException("Could not find corresponding target type for '{0}'", sourceType.Name);
                _typeMappings.Add(sourceType, targetType);
            }
            return targetType;
        }
    }
}
