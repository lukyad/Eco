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
    /// Provides custom parsing rules for the given field.
    /// 
    /// Parser contract:
    /// The Parser type that is passed as an argument to the attribute's constructor
    /// should define the following method:
    /// 
    ///        public static string Parse(string format, object source, FieldInfo context);
    /// 
    /// If parser fails, it should return null (ie do not throw).
    /// 
    /// Usage: 
    /// Can be applied to a field of a 'simple' type apart from String.
    /// Eco library allows usage of multiple Parser attributes for the same field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ParserAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ConverterAttribute),
            typeof(KnownTypesAttribute),
            typeof(RefAttribute)
        };

        public ParserAttribute(Type converterType)
            : this(converterType, format: null)
        {
        }

        public ParserAttribute(Type parserType, string format)
        {
            this.Type = parserType;
            this.Format = format;
            this.Parse = GetParseMethod(parserType);
        }

        public Type Type { get; set; }

        public string Format { get; set; }

        // object Parse(string source, string format, FiledInfo context)
        // Returns null, if converter is not able to parse the specified string.
        public Func<string, string, FieldInfo, object> Parse { get; private set; }

        // Can converter parse the given Type?
        public Func<Type, bool> CanParse { get; private set; }

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
            if (!context.FieldType.IsSimple() || context.FieldType == typeof(string))
                ThrowExpectedFieldOf("a simple non-String type", context);
            
            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }

        public static Func<string, string, FieldInfo, object> GetParseMethod(Type parserType)
        {
            MethodInfo parseMethod = parserType.GetMethod("Parse", new[] { typeof(string), typeof(string), typeof(FieldInfo) });
            if (parseMethod == null || parseMethod.ReturnType != typeof(object))
                ThrowMissingMethodException(parserType, "object Parse(string source, string format, FieldInfo context )");

            return (source, format, context) => parseMethod.Invoke(null, new object[] { source, format, context });
        }

        public static Func<Type, bool> GetCanParseMethod(Type converterType)
        {
            MethodInfo canParseMethod = converterType.GetMethod("CanParse", new[] { typeof(Type) });
            if (canParseMethod == null || canParseMethod.ReturnType != typeof(bool))
                ThrowMissingMethodException(converterType, "bool CanParse(Type sourceType)");

            return type => (bool)canParseMethod.Invoke(null, new[] { type });
        }
    }
}
