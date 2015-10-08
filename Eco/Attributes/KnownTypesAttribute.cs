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
    /// Can be used in combination with the PolymorphicAttribute. By default the ChoiceAttribute automatically
    /// includes all non-abstract types derived from the field's type plus field type itself (if it's not abstract)
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
    /// 
    /// Compatibility:
    /// Incompatible with the Id, ItemName, Ref and Converter attributes and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class KnownTypesAttribute : EcoFieldAttribute
    {
        readonly Type[] _ctorTypes;
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ChoiceAttribute),
            typeof(ConverterAttribute),
            typeof(ExternalAttribute),
            typeof(IdAttribute),
            typeof(ParserAttribute),
            typeof(RefAttribute),
        };

        public KnownTypesAttribute()
            : this(new Type[0])
        {
        }

        public KnownTypesAttribute(params Type[] types)
        {
            _ctorTypes = types;
        }

        public string Wildcard { get; set; }

        public IEnumerable<Type> GetKnownTypes(FieldInfo context)
        {
            foreach (var t in _ctorTypes) 
                yield return t;

            if (!String.IsNullOrEmpty(this.Wildcard))
            {
                var regexp = new Wildcard(this.Wildcard);
                Func<Type, bool> MatchesWildcard = t => regexp.Match(t.FullName).Success;
                var knownSettingsTypes = context.DeclaringType.Assembly.GetTypes().Where(t => MatchesWildcard(t) && t.IsSettingsType());
                foreach (var t in knownSettingsTypes)
                    yield return t;
            }
        }

        public override void ValidateContext(FieldInfo context)
        {
            if (!context.IsPolymorphic())
            {
                throw new ConfigurationException(
                    "{0} cannot be applied to {1}.{2}. Expected either a field of a settings array type or a field of a settings type marked with the ChoiceAttribute",
                    this.GetType().Name,
                    context.DeclaringType.Name,
                    context.Name
                );
            }
            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
