using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    /// <summary>
    /// Possible settins field usages.
    /// </summary>
    public enum Usage
    {
        // Indicates that the field is required by default and must be present in the configuration file.
        Required,
        // Indicates that the field is optional by default and can be omitted in the configuration file.
        Optional,
        // Indicates that the field is required, doesn't matter whether the usage attribute was specified or not.
        ForceRequired,
        // Indicates that the field is required, doesn't matter whether the usage attribute was specified or not.
        ForceOptional
    }
}
