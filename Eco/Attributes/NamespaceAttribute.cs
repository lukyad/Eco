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
    /// Instructs Eco library to apply the namespace provided by the field value
    /// to all other fields of a Non-Simple types.
    /// 
    /// Usage:
    /// Can be applied to a field of the 'String' type only. 
    /// Only one field in a class can be marked with the Namespace attribute.
    /// If Eco library detects more than one field marked with the Namespace attribute, 
    /// it throws an exception.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NamespaceAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(IdAttribute)
        };

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
            if (context.FieldType != typeof(string))
                ThrowExpectedFieldOf("the String type.", context);

            if (context.DeclaringType.GetFields().Count(f => f.IsDefined<NamespaceAttribute>()) > 1)
                throw new ConfigurationException($"Type {context.DeclaringType.Name} contains more than one field marked with the {nameof(NamespaceAttribute)}.");

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
