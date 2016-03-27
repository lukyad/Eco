using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;

namespace Eco
{
    /// <summary>
    /// Explicitily specifies known polymorphic types that can be serialized/deserialized for the given field.
    /// Can be used in combination with the PolymorphicAttribute. By default, the PolymorphicAttribute inctructs Eco library 
    /// to include all non-abstract types derived from the field's type plus field type itself (if it's not abstract)
    /// to the list of object types that can be serialized/deserialized. KnownTypesAttribute can limit this list
    /// to a certain types specified in the attribute's constructor.
    /// 
    /// The same rules apply to a field of an array type, i.e. by default all non-abstract types derived from the
    /// array's element type plus array element type itself (if it's not abstract) form the list of the polymorphic types
    /// known by serializer. KnowTypesAttributes can be used to limit this list to a certain types.
    /// 
    /// Usage:
    /// Can be applied to any polymorphic field. 
    /// (ie to a field satisfying the following criteria:
    ///         fieldType == typeof(object) || 
    ///         fieldType == typeof(object[]) ||
    ///         fieldType.IsSettingsType() && (fieldType.IsAbstract || field.IsDefined<PolymorphicAttribute>()) ||
    ///         fieldType.IsSettingsArrayType() && (fieldType.GetElementType().IsAbstract || field.IsDefined<PolymorphicAttribute>()
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class KnownTypesAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ConverterAttribute),
            typeof(IdAttribute),
            typeof(ParserAttribute),
            typeof(RefAttribute),
        };

        public KnownTypesAttribute(params Type[] list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            this.KnownTypes = list;
            this.ApplyToGeneratedClass = true;
        }

        public KnownTypesAttribute(string wildcard, Type context)
        {
            if (wildcard == null) throw new ArgumentNullException(nameof(wildcard));
            if (context == null) throw new ArgumentNullException(nameof(context));
            this.KnownTypes = MatchTypes(wildcard, context.Assembly).ToArray();
            this.ApplyToGeneratedClass = true;
        }


        public Type[] KnownTypes { get; protected set; }

        protected static IEnumerable<Type> MatchTypes(string wildcard, Assembly assembly)
        {
            var regexp = new Wildcard(wildcard);
            Func<Type, bool> MatchesWildcard = t => regexp.Match(t.FullName).Success;
            return assembly.GetTypes().Where(t => MatchesWildcard(t) && t.IsSettingsType());
        }

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
            if (!context.IsPolymorphic() && !context.FieldType.IsGenericType)
            {
                throw new ConfigurationException(
                    "{0} cannot be applied to {1}.{2}. Expected either field of a Polimorphoc type (please check IsPolimorphic() for details) or field of a Generic type.",
                    this.GetType().Name,
                    context.DeclaringType.Name,
                    context.Name
                );
            }
            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
