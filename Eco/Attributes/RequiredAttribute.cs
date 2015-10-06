﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Indicates that field is required.
    /// Results in an exception to be thrown if field value is missing (ie null) during serialization/desiarelization.
    /// 
    /// Usage:
    /// Can not be applied to fields of a value type and Nullable'1 type as during serialization/deserialization
    /// all fields of a value type are automaticvally marked with Required attribute and
    /// all fields of Nullable'1 type are automatically marked with Optional attribue.
    /// 
    /// Compatibility:
    /// Incompatible with Optional attribute and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(OptionalAttribute),
        };

        public override void ValidateContext(FieldInfo context)
        {
            if (context.FieldType.IsValueType || Nullable.GetUnderlyingType(context.FieldType) != null)
                ThrowExpectedFieldOf("a reference non-Nullable type", context);

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
