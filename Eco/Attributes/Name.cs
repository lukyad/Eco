using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Instructs serializer to override type or field name.
    /// Usage: 
    /// Can be applied to any settings type or field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public class NameAttribute : EcoFieldAttribute
    {
        public NameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
        }
    }
}
