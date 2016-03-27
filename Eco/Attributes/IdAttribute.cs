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
    /// Indicates that the field provides an unique configuration object name
    /// that can be used to reference the object in other places of a configuration file.
    /// 
    /// Usage:
    /// Can be applied to a field of the 'String' type only. 
    /// Only one field in a class can be marked with the Id attribute.
    /// If Eco library detects more than one field marked with the Id attribute, 
    /// it throws an exception.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IdAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(NamespaceAttribute)
        };

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
            if (context.FieldType != typeof(string))
                ThrowExpectedFieldOf("the String type.", context);

            if (context.DeclaringType.GetFields().Count(f => f.IsDefined<IdAttribute>()) > 1)
                throw new ConfigurationException($"Type {context.DeclaringType.Name} contains more than one field marked with the {nameof(IdAttribute)}.");

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
