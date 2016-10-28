using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Provides annotation for a settings class or for a settings class's field.
    /// Annotation gets included into configuration schema (only if target format supports schema definition).
    /// Currently supported by XmlSchemaExporter only.
    /// 
    /// Usage: 
    /// Can be applied to a class or to a field of any type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    public class DocAttribute : EcoFieldAttribute
    {
        public DocAttribute(string annotation)
        {
            this.Annotation = annotation;
            this.ApplyToGeneratedClass = true;
        }

        public string Annotation { get; }

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
            // Do nothing. Can be used in any context.
        }
    }
}
