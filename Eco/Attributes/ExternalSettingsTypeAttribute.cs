using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;

namespace Eco
{
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
