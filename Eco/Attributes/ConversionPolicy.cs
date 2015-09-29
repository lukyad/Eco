using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Provides custom serialization (ToString and FromString) methods for the given field.
    /// 
    /// Converter contract:
    /// The Converter type that is passed as an argument to the attribute's constructor
    /// should define the following two methods:
    ///        public static string ToString(string format, object source);
    ///        public static object FromString(string format, string source);
    /// 
    /// Usage: 
    /// Can be applied to a field of any type apart from String.
    /// 
    /// Compatibility: 
    /// Incompatible with the Id, Inline, ItemName, KnownTypes and Ref attributes and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConversionPolicyAttribute : EcoAttribute
    {
        public ConversionPolicyAttribute(Type sourceType, Type converterType)
            : this(sourceType, converterType, null)
        {
        }

        public ConversionPolicyAttribute(Type sourceType, Type converterType, string format)
        {
            this.ConverterType = converterType;
            this.Format = format;
        }

        public Type SourceType { get; set; }

        public Type ConverterType { get; set; }

        public string Format { get; set; }

        public bool Recursive { get; set; }
    }
}
