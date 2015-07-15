using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    public class DocAttribute : Attribute
    {
        public DocAttribute(string annotation)
        {
            this.Annotation = annotation;
        }

        public string Annotation { get; }
    }
}
