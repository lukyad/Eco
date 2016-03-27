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
    /// Can be applied to a field that has one of the following corresponding raw types: non-simple or string type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultAttribute : EcoFieldAttribute
    {
        public DefaultAttribute(object value)
        {
            this.Value = value;
            this.ApplyToGeneratedClass = true;
        }

        public object Value { get; }

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
            if (rawFieldType != typeof(string) && rawFieldType.IsSimple())
                ThrowExpectedFieldOf($"a type that has one of the following corresponding raw settings types: non-simple or string type. If orginal field is of a simple type, consider using of appropriate {nameof(ParsingPolicyAttribute)} attribute.", context);
        }
    }
}
