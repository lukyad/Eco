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
    [AttributeUsage(AttributeTargets.Field)]
    public class ConverterAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ChoiceAttribute),
            typeof(ExternalAttribute),
            typeof(InlineAttribute),
            typeof(ItemNameAttribute),
            typeof(KnownTypesAttribute),
            typeof(ParserAttribute),
            typeof(PolimorphicAttribute),
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
        }

        public Type Type { get; set; }

        public string Format { get; set; }

        // Returns string representation of the specified object.
        public new Func<object, string> ToString { get; private set; }

        // Should return null, if converter is not able to parse the specified string.
        public Func<string, object> FromString { get; private set; }

        public override void ValidateContext(FieldInfo context)
        {
            if (context.FieldType == typeof(string))
                ThrowExpectedFieldOf("a non-String type", context);

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }

        static Func<object, string> GetToStringMethod(Type converterType, string format)
        {
            MethodInfo toStringMethod = converterType.GetMethod("ToString", new[] { typeof(string), typeof(object) });
            if (toStringMethod == null || toStringMethod.ReturnType != typeof(string))
                ThrowMissingMethodException(converterType, "string ToString(string format, object source)");
            
            return value => (string)toStringMethod.Invoke(null, new[] { format, value });
        }

        static Func<string, object> GetFromStringMethod(Type converterType, string format)
        {
            MethodInfo fromStringMethod = converterType.GetMethod("FromString", new[] { typeof(string), typeof(string) });
            if (fromStringMethod == null || fromStringMethod.ReturnType != typeof(object))
                ThrowMissingMethodException(converterType, "object FromString(string format, string source)");

            return str => fromStringMethod.Invoke(null, new[] { format, str });
        }

        static void ThrowMissingMethodException(Type methodContainer, string methodSignature)
        {
            throw new ConfigurationException("{0} type doesn't have required method: {1}", methodContainer.Name, methodSignature);
        }
    }
}
