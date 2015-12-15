using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Instructs serializer to override field name.
    /// Usage: 
    /// Can be applied to any field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NameAttribute : EcoFieldAttribute
    {
        public NameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override void ValidateContext(FieldInfo context)
        {
        }
    }
}
