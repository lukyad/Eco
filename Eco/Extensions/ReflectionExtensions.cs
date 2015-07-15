using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Serialization;
using Eco.Attributes;

namespace Eco.Extensions
{
    public static class ReflectionExtensions
    {
        //public static bool HasAttribute<T>(this Assembly a)
        //{
        //	return Attribute.IsDefined(a, typeof(T));
        //}

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

		public static string GetOverridenName(this FieldInfo field)
		{
			//var atributeAttr = field.GetCustomAttribute<XmlAttributeAttribute>(false);
			//var elementAttr = field.GetCustomAttribute<XmlElementAttribute>(false);
			//var arrayAttr = field.GetCustomAttribute<XmlArrayAttribute>(false);
			//if (atributeAttr != null && !String.IsNullOrEmpty(atributeAttr.AttributeName)) return atributeAttr.AttributeName;
			//else if (elementAttr != null && !String.IsNullOrEmpty(elementAttr.ElementName)) return elementAttr.ElementName;
			//else if (arrayAttr != null && !String.IsNullOrEmpty(arrayAttr.ElementName)) return arrayAttr.ElementName;
			//else return field.Name;
			return field.Name;
		}

		// Returns a friendly type name that can be understood by C# compiler
		public static string GetFriendlyName(this Type type, string typeNamesapce)
		{
			string friendlyName = type.Name;
			if (type.IsGenericType)
			{
				int iBacktick = friendlyName.IndexOf('`');
				if (iBacktick > 0)
				{
					friendlyName = friendlyName.Remove(iBacktick);
				}
				friendlyName += "<";
				Type[] typeParameters = type.GetGenericArguments();
				for (int i = 0; i < typeParameters.Length; ++i)
				{
					string typeParamName = GetFriendlyName(typeParameters[i], typeNamesapce);
					friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
				}
				friendlyName += ">";
			}

			return typeNamesapce + "." + friendlyName.Replace('+', '.');
		}

		public static IEnumerable<Type> GetReferencedSettingsTypesRecursive(this Type root)
		{
			return GetReferencedSettingsTypesRecursive(root, new HashSet<Type>());
		}

		static IEnumerable<Type> GetReferencedSettingsTypesRecursive(Type root, HashSet<Type> visitedTypes)
		{
			if (root.IsSettingsType())
			{
				yield return root;
				visitedTypes.Add(root);

				foreach (var type in GetReferencedSettingsTypes(root))
				{
					if (!visitedTypes.Contains(type))
					{
						foreach (var t in GetReferencedSettingsTypesRecursive(type, visitedTypes))
							yield return t;
					}
				}
			}
		}

		static IEnumerable<Type> GetReferencedSettingsTypes(Type type)
		{
			if (type.BaseType.IsSettingsType())
				yield return type.BaseType;

			foreach (var field in type.GetOwnFields())
			{
				Type settingsType = null;
				if (field.FieldType.IsSettingsType())
					settingsType = field.FieldType;
				else if (field.FieldType.IsSettingsArrayType())
					settingsType = field.FieldType.GetElementType();

				if (settingsType != null)
				{
					yield return settingsType;
					foreach (var t in settingsType.GetDerivedTypes())
						yield return t;
				}

				foreach (var t in GetSerializableTypes(field).Where(t => t.IsSettingsType()))
					yield return t;
			}
		}

		public static IEnumerable<Type> GetSerializableTypes(this FieldInfo field)
		{
			var knownTypesAttributes = field.GetCustomAttributes<KnownTypesAttribute>().ToArray();
			if (knownTypesAttributes.Length > 0)
			{
				foreach (var a in knownTypesAttributes)
				{
					var knownTypes = a.GetAllKnownTypes(field).Where(t => !t.IsAbstract);
                    foreach (var t in knownTypes)
						yield return t;
				}
			}
			else
			{
				Type baseType;
				if (field.FieldType.IsArray) baseType = field.FieldType.GetElementType();
				else baseType = field.FieldType;

				var knownTypes =
					baseType.GetDerivedTypes()
					.Append(baseType)
					.Where(t => !t.IsAbstract);

				foreach (var t in knownTypes)
					yield return t;
            }
		}

		static readonly Type[] _simpleXmlTypes = new[] {
				typeof(string), typeof(bool), typeof(sbyte), typeof(byte), typeof(DateTime), typeof(decimal), typeof(double),
				typeof(float), typeof(int), typeof(long), typeof(short), typeof(uint), typeof(ulong), typeof(ushort) };

		public static bool IsSimple(this Type type)
		{
			return _simpleXmlTypes.Contains(type) || type != null && type.IsEnum;
        }
    }
}
