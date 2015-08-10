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
    /// Instructs IFieldVisitors to skip any post-processing of the given field.
    /// (e.g. EnvironmnetVariableExpander would skip any fields marked as Sealed)
    /// 
    /// Usage:
    /// Can be applied to a field of any type.
    /// 
    /// Compatibility:
    /// Incomaptible with the Converter and Ref attributes and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SealedAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ConverterAttribute),
            typeof(RefAttribute),
        };

        public override void ValidateContext(FieldInfo context)
        {
            AttributeValidator.CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
