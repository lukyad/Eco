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
    /// Instructs the IRawFieldVisitors to skip any processing of the given field.
    /// (e.g. EnvironmnetVariableExpander would skip any fields marked as Sealed)
    /// 
    /// Usage:
    /// Can be applied to a field of any type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SealedAttribute : EcoFieldAttribute
    {
        public SealedAttribute()
        {
            this.ApplyToGeneratedClass = true;
        }

        public override void ValidateContext(FieldInfo context)
        {
            // do nothing. can be applied to field of any type and is compatible with all Eco attributes.
        }
    }
}
