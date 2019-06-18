using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Eco.Serialization
{
    // Classes marked with this attribute will be automatically added to the SupportedFormats list.
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsSerializerAttribute : Attribute
    {
        public SettingsSerializerAttribute(string format)
        {
            Format = format ?? throw new ConfigurationException($"{nameof(format)} argument must not be null or empty.");
        }

        public string Format { get; }
    }
}
