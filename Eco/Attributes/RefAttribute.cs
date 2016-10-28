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
    /// If referenced object was not found, an exception would be thrown by default.
    /// You can change this behaviour by setting the Weak property to true. In this case,
    /// if referenced object was not found, the reverence would be resolved to null.
    /// 
    /// 
    /// Usage:
    /// Can be applied to a field of a settings type or a settings array type only. The referenced type (underlying field type)
    /// must have a field marked with IdAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RefAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ConverterAttribute),
            typeof(IdAttribute),
            typeof(InlineAttribute),
            typeof(RenameAttribute),
            typeof(KnownTypesAttribute),
            typeof(ParserAttribute),
            typeof(PolymorphicAttribute)
        };

        public RefAttribute()
        {
            this.IsWeak = false;
        }

        public bool IsWeak { get; set; }

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
            Type fieldType = context.FieldType;
            if (!fieldType.IsSettingsOrObjectType() && !fieldType.IsSettingsOrObjectArrayType())
                ThrowExpectedFieldOf("a settings/object or a settings/object array type", context);
            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
