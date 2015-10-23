using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Applies to a field of a polymorphic type only.
    /// Instructs serializer to rename any known polymorphic type according to the specified rule.
    /// 
    /// Usage:
    /// Can be applied to a field of a polymorphic type only.
    /// 
    /// Compatibility:
    /// Incomaptible with KnownTypes and Ref attributes and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RenameAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ConverterAttribute),
            typeof(IdAttribute),
            typeof(ParserAttribute),
            typeof(RefAttribute)
        };

        public RenameAttribute(string pattern, string replacement)
        {
            EnsureNonEmptyArgument(pattern, nameof(pattern));
            EnsureNonEmptyArgument(replacement, nameof(replacement));

            this.Pattern = pattern;
            this.Replacement = replacement;
            this.KeepOriginalCamelCase = true;
        }

        static void EnsureNonEmptyArgument(string arg, string argName)
        {
            if (String.IsNullOrEmpty(arg)) throw new ConfigurationException($"Invalid {argName} argument. Expected non-empty string");
        }

        /// <summary>
        /// Regex pattern to match.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// String to placed inplace of the matched regex.
        /// </summary>
        public string Replacement { get; private set; }

        /// <summary>
        /// If set to true, the result string will have the first char in the same case (upper or lower) as original string.
        /// True, by default.
        /// </summary>
        public bool KeepOriginalCamelCase { get; set; }

        public override void ValidateContext(FieldInfo context)
        {
            if (!context.FieldType.IsArray)
                ThrowExpectedFieldOf("an array type", context);

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }

        public string Rename(string input)
        {
            if (String.IsNullOrEmpty(input)) throw new ConfigurationException("Expected non-empty string");

            string result = Regex.Replace(input, this.Pattern, this.Replacement);
            if (this.KeepOriginalCamelCase)
            {
                if (Char.IsLower(input[0])) result = Char.ToLower(result[0]) + result.Substring(1);
                else result = Char.ToUpper(result[0]) + result.Substring(1);
            }
            return result;
        }
    }
}
