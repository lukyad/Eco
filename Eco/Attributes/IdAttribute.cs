using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Indicates that the field provides an unique configuration object name
    /// that can be used to reference the object in other places of a configuration file.
    /// 
    /// Usage:
    /// Can be only applied to a field of type String.
    /// 
    /// Compatibility:
    /// Incompatible with: ChoiceAttribute, InlineAttribute, ItemNameAttribute, KnownTypesAttribute, RefAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IdAttribute : Attribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ChoiceAttribute),
            typeof(InlineAttribute),
            typeof(ItemNameAttribute),
            typeof(KnownTypesAttribute),
            typeof(RefAttribute)
        };

        public static void ValidateContext(FieldInfo context)
        {
            if (context.FieldType != typeof(string))
            {
                throw new ConfigurationException(
                    "{0} cannot be applied to {1}.{2}. Expected field of the String type",
                    typeof(IdAttribute).Name,
                    context.DeclaringType.Name,
                    context.Name
                );
            }
            AttributeValidator.CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
