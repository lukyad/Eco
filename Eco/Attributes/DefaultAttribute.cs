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
    /// Provides default value for the raw field.
    /// 
    /// Usage:
    /// Can be applied to a field of any type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultAttribute : EcoFieldAttribute
    {
        public DefaultAttribute(object value)
        {
            this.Value = value;
        }

        public object Value { get; }

        public override void ValidateContext(FieldInfo context)
        {
            // do nothing. can be applied to field of any type and is compatible with all Eco attributes.
        }
    }
}
