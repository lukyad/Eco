using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    /// <summary>
    /// Indicates that the given type can be the root serializing type.
    /// Can be used by Attributes generator to emit some extra serialization attributes
    /// required by the serializer. (eg XmlRoot).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RootAttribute : EcoAttribute
    {
    }
}
