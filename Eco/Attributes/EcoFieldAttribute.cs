using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Base class for all Eco field attributes.
    /// </summary>
    public abstract class EcoFieldAttribute : EcoAttribute
    {
        public abstract void ValidateContext(FieldInfo context);

        protected void ThrowExpectedFieldOf(string type, FieldInfo context)
        {
            throw new ConfigurationException(
                "{0} cannot be applied to {1}.{2}. Expected field of {3}",
                this.GetType().Name,
                context.DeclaringType.Name,
                context.Name,
                type
            );
        }
    }
}
