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
        /// <summary>
        /// If set to true, the attribute will be applied to the raw settings type generated in runtime by the Eco librarary.
        /// </summary>
        public bool ApplyToGeneratedClass { get; protected set; } = false;

        public abstract void ValidateContext(FieldInfo context, Type rawFieldType);

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
        protected static void ThrowMissingMethodException(Type methodContainer, string methodSignature)
        {
            throw new ConfigurationException("{0} type doesn't have required method: {1}.", methodContainer.Name, methodSignature);
        }
    }
}
