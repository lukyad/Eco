using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Serialization;

namespace Eco.Extensions
{
    public static class ReflectionExtensions
    {
        public static bool IsDefined<T>(this MemberInfo m, bool inherit = false)
        {
            return m.IsDefined(typeof(T), inherit);
        }

        public static bool IsSettingsType(this Type t)
        {
            if (t.IsArray || !t.IsClass) return false;

            var settingsAssemblyAttr = t.Assembly.GetCustomAttribute<SettingsAssemblyAttribute>();
            return
                settingsAssemblyAttr != null && (
                String.IsNullOrEmpty(settingsAssemblyAttr.SettingsTypesNamesapace) ||
                t.Namespace.StartsWith(settingsAssemblyAttr.SettingsTypesNamesapace));
        }

        public static bool IsSettingsArrayType(this Type t)
        {
            if (!t.IsArray) return false;
            return t.GetElementType().IsSettingsType();
        }

        public static bool IsPolymorphic(this FieldInfo field)
        {
            var fieldType = field.FieldType;
            return 
                fieldType == typeof(object) || fieldType == typeof(object[]) ||
                fieldType.IsSettingsType() && (fieldType.IsAbstract || field.IsDefined<PolymorphicAttribute>()) ||
                fieldType.IsSettingsArrayType() && (fieldType.GetElementType().IsAbstract || field.IsDefined<PolymorphicAttribute>());
        }

        public static IEnumerable<Type> GetDerivedTypes(this Type type)
        {
             return type.Assembly.GetTypes().Where(t => t.IsSubclassOf(type));
        }

        public static IEnumerable<FieldInfo> GetOwnFields(this Type type)
        {
            IEnumerable<FieldInfo> ownFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (type.BaseType != null)
            {
                var baseFields = type.BaseType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                ownFields = ownFields.Where(of => baseFields.All(bf => bf.Name != of.Name));
            }
            return ownFields;
        }

        public static string GetNonGenericName(this Type type)
        {
            string nonGenericName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = nonGenericName.IndexOf('`');
                if (iBacktick > 0)
                {
                    nonGenericName = nonGenericName.Remove(iBacktick);
                }
                nonGenericName += "__";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    Type typeParam = typeParameters[i];
                    string typeParamName = typeParam.IsSettingsType() || typeParam.IsSettingsArrayType() ?
                        GetNonGenericName(typeParameters[i]) :
                        typeParam.FullName;
                    nonGenericName += i == 0 ? typeParamName : "__" + typeParamName.Replace('.', '_');
                }
            }

            return nonGenericName;
        }

        public static IEnumerable<Type> GetReferencedSettingsTypesRecursive(this Type root)
        {
            return GetReferencedSettingsTypesRecursive(root, new HashSet<Type>());
        }

        static IEnumerable<Type> GetReferencedSettingsTypesRecursive(Type rootType, HashSet<Type> visitedTypes)
        {
            if (rootType != null && rootType.IsSettingsType() && !visitedTypes.Contains(rootType))
            {
                yield return rootType;
                visitedTypes.Add(rootType);

                foreach (var type in GetReferencedTypes(rootType))
                {
                    Type settingsType = null;
                    if (type.IsSettingsType())
                        settingsType = type;
                    else if (type.IsSettingsArrayType())
                        settingsType = type.GetElementType();

                    foreach (var t in GetReferencedSettingsTypesRecursive(settingsType, visitedTypes))
                        yield return t;
                }

                foreach (var derivedType in rootType.GetDerivedTypes())
                {
                    foreach (var t in GetReferencedSettingsTypesRecursive(derivedType, visitedTypes))
                        yield return t;
                }
            }
        }

        static IEnumerable<Type> GetReferencedTypes(Type type)
        {
            yield return type.BaseType;

            foreach (var field in type.GetOwnFields())
            {
                foreach (var t in GetKnownSerializableTypes(field))
                    yield return t;

                if (field.IsDefined<FieldMutatorAttribute>())
                    yield return field.GetCustomAttribute<FieldMutatorAttribute>().GetRawSettingsFieldType(field);
            }
        }

        public static IEnumerable<Type> GetKnownSerializableTypes(this FieldInfo field)
        {
            var knownTypesAttributes = field.GetCustomAttributes<KnownTypesAttribute>().ToArray();
            IEnumerable<Type> knownTypes = Enumerable.Empty<Type>();
            if (knownTypesAttributes.Length > 0)
            {
                foreach (var a in knownTypesAttributes)
                    knownTypes = knownTypes.Concat(a.GetKnownTypes(field));
            }
            else
            {
                Type baseType;
                if (field.FieldType.IsArray) baseType = field.FieldType.GetElementType();
                else baseType = field.FieldType;

                knownTypes = baseType.GetDerivedTypes().Append(baseType);
            }

            var serializableKnownTypes = knownTypes.Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition);
            foreach (var t in serializableKnownTypes)
                yield return t;
        }

        static readonly Type[] _simpleXmlTypes = new[] {
                typeof(string), typeof(bool), typeof(sbyte), typeof(byte), typeof(DateTime), typeof(decimal), typeof(double),
                typeof(float), typeof(int), typeof(long), typeof(short), typeof(uint), typeof(ulong), typeof(ushort) };

        public static bool IsSimple(this Type type)
        {
            return _simpleXmlTypes.Contains(type) || type != null && type.IsEnum;
        }

        public static object GetFieldValue(this object container, string fieldName)
        {
            return container.GetType().GetField(fieldName).GetValue(container);
        }
    }
}
