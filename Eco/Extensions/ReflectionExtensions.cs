using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;

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
            return SettingsTypeChecker.IsSettingsType(t);
        }

        public static bool IsSettingsOrObjectType(this Type t)
        {
            if (t == typeof(object)) return true;
            return IsSettingsType(t);
        }

        public static bool IsSettingsArrayType(this Type t)
        {
            if (!t.IsArray) return false;
            return t.GetElementType().IsSettingsType();
        }

        public static bool IsSettingsOrObjectArrayType(this Type t)
        {
            if (t == typeof(object[])) return true;
            return IsSettingsArrayType(t);
        }

        public static bool IsNullable(this Type t)
        {
            return Nullable.GetUnderlyingType(t) != null;
        }

        public static bool IsPolymorphic(this FieldInfo field)
        {
            var fieldType = field.FieldType;
            return 
                fieldType == typeof(object) || fieldType == typeof(object[]) ||
                fieldType.IsSettingsType() && (fieldType.IsAbstract || field.IsDefined<PolymorphicAttribute>()) ||
                fieldType.IsSettingsArrayType() && (fieldType.GetElementType().IsAbstract || field.IsDefined<PolymorphicAttribute>());
        }

        public static bool IsEcoElementOfType<T>(this object element)
        {
            return element?.GetType().GetCustomAttribute<EcoElementAttribute>()?.ElementType == typeof(T);
        }

        public static bool IsEcoElementOfGenericType(this object element, Type genericTypeDefinition)
        {
            var elementType = element.GetType().GetCustomAttribute<EcoElementAttribute>()?.ElementType;
            return
                elementType != null &&
                elementType.IsGenericTypeDefinition &&
                elementType == genericTypeDefinition;
        }

        public static IEnumerable<Type> GetDerivedTypes(this Type type)
        {
             return TypesCache.GetAssemblyTypes(type.Assembly).Where(t => t.IsSubclassOf(type));
        }

        public static IEnumerable<Type> GetBaseSettingsTypes(this Type type)
        {
            Type super = type.BaseType;
            while (super.IsSettingsType())
            {
                yield return super;
                super = super.BaseType;
            }
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
            if (type.IsArray && type.GetElementType().IsGenericType)
                return GetNonGenericName(type.GetElementType()) + "[]";

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
                    string normalizedTypeName = typeParamName.Replace('.', '_').Replace("[]", "Array");
                    nonGenericName += i == 0 ? normalizedTypeName : "__" + normalizedTypeName;
                }
            }

            return nonGenericName;
        }

        public static string GetFullCsharpCompatibleName(this Type type)
        {
            var compatibleName = type.FullName;
            if (!type.IsGenericType) return compatibleName;

            var iBacktick = compatibleName.IndexOf('`');
            if (iBacktick > 0) compatibleName = compatibleName.Remove(iBacktick);

            var genericParameters = type.GetGenericArguments().Select(x => type.IsGenericTypeDefinition ? "" : x.GetFullCsharpCompatibleName());
            compatibleName += "<" + string.Join(", ", genericParameters) + ">";

            return compatibleName;
        }

        public static Type[] GetReferencedSettingsTypesRecursive(this Type root)
        {
            return TypesCache.GetReferencedSettingsTypes(root);
        }

        public static Type[] GetKnownSerializableTypes(this FieldInfo field)
        {
            return TypesCache.GetKnownSerializableTypes(field);
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

        public static void SetFieldValue(this object container, string fieldName, object value)
        {
            container.GetType().GetField(fieldName).SetValue(container, value);
        }
    }
}
