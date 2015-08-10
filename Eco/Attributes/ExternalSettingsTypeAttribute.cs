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
    /// Specifies raw settings type to be loaded from an external settings file being included to the configuration file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ExternalSettingsTypeAttribute : Attribute
    {
        public ExternalSettingsTypeAttribute(Type type)
        {
            this.Type = type;
        }

        public Type Type { get; private set; }
    }
}
