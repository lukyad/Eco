using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Converters;

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
        public static ParsingPolicyAttribute[] DefaultParsingPolicies { get; } = new ParsingPolicyAttribute[]
        {
            new ParsingPolicyAttribute(typeof(TimeSpanConverter)),
            new ParsingPolicyAttribute(typeof(NumberConverter))
        };

        public ParsingPolicyAttribute[] ParsingPolicies => this.DisableDefaultParsingPolicies ? new ParsingPolicyAttribute[0] : DefaultParsingPolicies;

        public bool DisableDefaultParsingPolicies { get; set; }
    }
}
