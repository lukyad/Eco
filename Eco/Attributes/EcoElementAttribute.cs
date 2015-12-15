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
    /// Used internally by the Eco library.
    /// Applies to all Eco elements;
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EcoElementAttribute : EcoAttribute
    {
        public EcoElementAttribute(Type elementType)
        {
            if (elementType == null) throw new ArgumentNullException(nameof(elementType));
            this.ElementType = elementType;
        }

        public Type ElementType { get; }
    }
}
