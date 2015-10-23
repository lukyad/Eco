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
    /// Instructs the ISerializer to skip serialization of this field.
    /// It also won't appear in the schema (if any)
    /// 
    /// Usage:
    /// Can be applied to a field of any type.
    /// 
    /// Compatibility:
    /// Compatible with all Eco attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IgnoreAttribute : EcoFieldAttribute
    {
        public override void ValidateContext(FieldInfo context)
        {
            // do nothing. can be applied to field of any type and is compatible with all Eco attributes.
        }
    }
}
