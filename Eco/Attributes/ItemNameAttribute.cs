using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Instructs serializer to always serialize items of the given array with the specified name.
    /// 
    /// Usage:
    /// Can be applied to a field of an array type only.
    /// 
    /// Compatibility:
    /// Incomaptible with KnownTypes and Ref attributes and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ItemNameAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(KnownTypesAttribute),
            typeof(RefAttribute)
        };

        public ItemNameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public override void ValidateContext(FieldInfo context)
        {
            if (!context.FieldType.IsArray)
                base.ThrowExpectedFieldOf("an array type", context);

            AttributeValidator.CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
