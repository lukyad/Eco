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
    /// Base class for all Eco field attributes.
    /// </summary>
    public abstract class EcoFieldAttribute : EcoAttribute
    {
        public abstract void ValidateContext(FieldInfo context);

        protected void ThrowExpectedFieldOf(string type, FieldInfo context)
        {
            throw new ConfigurationException(
                "{0} cannot be applied to {1}.{2}. Expected field of the {3} type",
                this.GetType().Name,
                context.DeclaringType.Name,
                context.Name,
                type
            );
        }
        protected static void ThrowMissingMethodException(Type methodContainer, string methodSignature)
        {
            throw new ConfigurationException("{0} type doesn't have required method: {1}", methodContainer.Name, methodSignature);
        }
    }
}
