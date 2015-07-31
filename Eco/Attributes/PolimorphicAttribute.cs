using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;

namespace Eco
{
    // <summary>
    /// Indicates that the given field can contain polymorphic objects.
    /// By default the list of permitted polymorphic types includes
    /// all non-abstract types derived from the field type 
    /// plus field type itself, if it's not abstract.
    /// 
    /// If field is of an abstract settings type then it's already polimorphic
    /// and the Polimorphic attribute can be omitted
    /// 
    /// The list of permitted polymorphic types can be limited with
    /// the KnownTypes attribute.
    /// 
    /// Usage:
    /// Can be applied to any field of a non-abstract settings type (any type from the assembly marked with SettingsAssembly attribute)
    /// 
    /// Compatibility:
    /// Incompatible with the Id, Inline, ItemName, Converter and Ref attributes and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PolimorphicAttribute : Attribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(IdAttribute),
            typeof(InlineAttribute),
            typeof(ItemNameAttribute),
            typeof(ConverterAttribute),
            typeof(RefAttribute)
        };

        public void ValidateContext(FieldInfo context)
        {
            if (!context.FieldType.IsSettingsType() || !context.FieldType.IsAbstract)
            {
                throw new ConfigurationException(
                    "{0} cannot be applied to {1}.{2}. Expected field of a non-abstract settings type",
                    typeof(ChoiceAttribute).Name,
                    context.DeclaringType.Name,
                    context.Name
                );
            }
            AttributeValidator.CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
