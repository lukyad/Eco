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
    /// Instructs specified visitors to skip processing of the target field.
    /// 
    /// Usage:
    /// Can be applied to a field of any type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SkippedByAttribute : EcoFieldAttribute
    {
        public SkippedByAttribute(params Type[] visitors)
        {
            this.Visitors = new HashSet<Type>(visitors);
            this.ApplyToGeneratedClass = true;
        }

        public HashSet<Type> Visitors { get; }

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
            // do nothing. can be applied to field of any type and is compatible with all Eco attributes.
        }
    }
}
