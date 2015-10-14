using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Defines parsing rules for 'simple' types to be used when reading configuration files.
    /// 
    /// Parser class contract:
    /// The Parser class that is passed as an argument to the attribute's constructor
    /// should define the following method:
    /// 
    ///        public static object Parse(string source, string format);
    /// 
    /// If parser fails, it should return null (ie do not throw).
    /// 
    /// Usage: 
    /// Should be applied to a root settings type. The rool is propogated to all referenced settings types recursively.
    /// Eco library allows usage of multiple ParsingPolicy attributes for the same source type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ParsingPolicyAttribute : EcoAttribute
    {
        public ParsingPolicyAttribute(Type parserType)
            : this(parserType, null)
        {
        }

        public ParsingPolicyAttribute(Type parserType, string format)
        {
            this.Format = format;
            this.Parse = ParserAttribute.GetParseMethod(parserType);
            this.CanParse = ParserAttribute.GetCanParseMethod(parserType);
        }

        public string Format { get; set; }

        // Returns null, if converter is not able to parse the specified string.
        public Func<string, string, object> Parse { get; private set; }

        // Can converter parse the given Type?
        public Func<Type, bool> CanParse { get; private set; }
    }
}
