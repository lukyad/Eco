using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;
using Eco.SettingsVisitors;

namespace Eco
{
    /// <summary>
    /// Internal implementation class used by the Eco library.
    /// </summary>
    static class SettingsConstruction
    {
        public static object CreateSettingsObject(object sourceObject, FieldInfo targetField, Dictionary<Type, Type> typeMappings)
        {
            Type sourceObjectType = sourceObject.GetType();
            Type targetType = GetMatchingTargetType(sourceObjectType, targetField, typeMappings);
            return Activator.CreateInstance(targetType);
        }

        public static object CreateSettingsArray(Array sourceArray, FieldInfo targetField, Dictionary<Type, Type> typeMappings)
        {
            int sourceArrayLength = sourceArray.Length;
            Array targetArray = Array.CreateInstance(targetField.FieldType.GetElementType(), sourceArrayLength);
            for (int i = 0; i < targetArray.Length; i++)
            {
                Type sourceElementType = sourceArray.GetValue(i).GetType();
                Type targetElementType = GetMatchingTargetType(sourceElementType, targetField, typeMappings);
                targetArray.SetValue(Activator.CreateInstance(targetElementType), i);
            }
            return targetArray;
        }

        static Type GetMatchingTargetType(Type sourceType, FieldInfo targetField, Dictionary<Type, Type> typeMappings)
        {
            Type targetType;
            if (!typeMappings.TryGetValue(sourceType, out targetType))
            {
                targetType = targetField.DeclaringType
                    .GetReferencedSettingsTypesRecursive()
                    .FirstOrDefault(t => t.NonGenericName() == sourceType.NonGenericName());
                if (targetType == null) throw new ConfigurationException("Could not find corresponding target type for '{0}'.", sourceType.Name);
                typeMappings.Add(sourceType, targetType);
            }
            return targetType;
        }
    }
}
