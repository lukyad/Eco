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
    /// Indicates that field is optinal.
    /// Allows field value to be missing (ie null) during serialization/desiarelization.
    /// 
    /// Usage:
    /// Can not be applied to fields of a value type and Nullable'1 type as during serialization/deserialization
    /// all fields of a value type are automaticvally marked with Required attribute and
    /// all fields of Nullable'1 type are automatically marked with Optional attribue.
    /// 
    /// Compatibility:
    /// Incompatible with Required attribute and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OptionalAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(RequiredAttribute),
        };

        public override void ValidateContext(FieldInfo context)
        {
            if (context.FieldType.IsValueType || Nullable.GetUnderlyingType(context.FieldType) != null)
                ThrowExpectedFieldOf("a reference non-Nullable type", context);

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
