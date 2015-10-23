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
    public class IdAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(InlineAttribute),
            typeof(RenameAttribute),
            typeof(KnownTypesAttribute),
            typeof(RefAttribute)
        };

        public override void ValidateContext(FieldInfo context)
        {
            if (context.FieldType != typeof(string))
                ThrowExpectedFieldOf("the String type", context);

            // Do not check attribute compatibility as it compatible  with all existing Eco attributes
            // that can be applied to a field of the String type.
        }
    }
}
