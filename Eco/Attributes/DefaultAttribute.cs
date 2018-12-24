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
    /// Can be applied to a field that has the following attributes: (raw type is string) and (field type is either Nullable or not a value type or field is hidden).
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
            //&& Nullable.GetUnderlyingType(field.FieldType) == null
            if (rawFieldType != typeof(string) || (context.FieldType.IsValueType && Nullable.GetUnderlyingType(context.FieldType) == null && !context.IsDefined<HiddenAttribute>()))
                ThrowExpectedFieldOf($"a type that has one of the following attributes: (raw type is string) and (field type is either Nullable or not a value type or field is hidden).", context);
        }
    }
}
