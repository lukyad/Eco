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
    /// Indicates that the given field is a reference to another settings object defined in the configuration files.
    /// Field marked with RefAttribute is always serialized as a String containing either a signle name of the 
    /// referenced object or comma-separated list of names if underlying field type is an settings array.
    /// 
    /// Usage:
    /// Can be applied to a field of a settings type or a settings array type only. The referenced type (underlying field type)
    /// must have a field marked with IdAttribute.
    /// 
    /// Compatibility:
    /// Incomaptible with the Id, Inline, ItemName, KnownTypes and Converter attributes and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RefAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ChoiceAttribute),
            typeof(ConverterAttribute),
            typeof(ExternalAttribute),
            typeof(IdAttribute),
            typeof(InlineAttribute),
            typeof(ItemNameAttribute),
            typeof(KnownTypesAttribute),
            typeof(ParserAttribute),
            typeof(PolimorphicAttribute)
        };

        public override void ValidateContext(FieldInfo context)
        {
            if (!context.FieldType.IsSettingsType() && !context.FieldType.IsSettingsArrayType())
                ThrowExpectedFieldOf("a settings type or a settings array type", context);

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
