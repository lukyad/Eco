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
    /// Provides custom serialization (ToString and FromString) methods for the given field.
    /// 
    /// Converter contract:
    /// The Converter type that is passed as an argument to the attribute's constructor
    /// should define the following two methods:
    ///        public static bool CanParse(Type sourceType);
    ///        public static string ToString(object source, string format, FieldInfo context);
    ///        public static object FromString(string source, string format,  FieldInfo context);
    /// 
    /// Usage: 
    /// Can be applied to a field of any type apart from System.String and Eco.include
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ConverterAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(InlineAttribute),
            typeof(RenameAttribute),
            typeof(KnownTypesAttribute),
            typeof(ParserAttribute),
            typeof(PolymorphicAttribute),
            typeof(RefAttribute)
        };

        public ConverterAttribute(Type converterType)
            : this(converterType, format: null)
        {
        }

        public ConverterAttribute(Type converterType, string format)
        {
            this.Type = converterType;
            this.Format = format;
            this.ToString = GetToStringMethod(converterType, format);
            this.FromString = GetFromStringMethod(converterType, format);
            this.CanParse = ParserAttribute.GetCanParseMethod(converterType);
        }

        public Type Type { get; set; }

        public string Format { get; set; }

        // string ToString(object source, FieldInfo context)
        // Returns string representation of the specified object.
        public new Func<object, FieldInfo, string> ToString { get; private set; }

        // object FromString(object source, FieldInfo context)
        // Should return null, if converter is not able to parse the specified string.
        public Func<string, FieldInfo, object> FromString { get; private set; }

        // Can converter parse the given TYpe.
        public Func<Type, bool> CanParse { get; private set; }

        public override void ValidateContext(FieldInfo context, Type rawFieldType)
        {
            if (context.FieldType.IsDefined<EcoElementAttribute>())
                ThrowExpectedFieldOf("any type apart from System.String and any of the Eco configuration element types.", context);
            if (!CanParse(context.FieldType))
                new ConfigurationException("Invalid Converter type for the {0}.{1} field.", context.DeclaringType.Name, context.Name);

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }

        static Func<object, FieldInfo, string> GetToStringMethod(Type converterType, string format)
        {
            MethodInfo toStringMethod = converterType.GetMethod("ToString", new[] { typeof(object), typeof(string), typeof(FieldInfo) });
            if (toStringMethod == null || toStringMethod.ReturnType != typeof(string))
                ThrowMissingMethodException(converterType, "string ToString(object source, string format, FieldInfo context)");
            
            return (value, context) => (string)toStringMethod.Invoke(null, new[] { value, format, context });
        }

        static Func<string, FieldInfo, object> GetFromStringMethod(Type converterType, string format)
        {
            MethodInfo fromStringMethod = converterType.GetMethod("FromString", new[] { typeof(string), typeof(string), typeof(FieldInfo) });
            if (fromStringMethod == null || fromStringMethod.ReturnType != typeof(object))
                ThrowMissingMethodException(converterType, "object FromString(string format, string source, FieldInfo context)");

            return (str, context) => fromStringMethod.Invoke(null, new object[] { str, format, context });
        }
    }
}
