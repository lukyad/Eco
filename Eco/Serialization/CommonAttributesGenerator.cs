using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;
using Eco.CodeBuilder;

namespace Eco.Serialization.Xml
{
    public class CommonAttributesGenerator : ISerializationAttributesGenerator
    {
        public virtual IEnumerable<string> GetAttributesTextFor(Type settingsType)
        {
            try
            {
                var result = new List<string>();
                var attributes = settingsType.GetCustomAttributes().ToArray();
                var inheritedAttributesData = settingsType.GetBaseSettingsTypes().SelectMany(t => t.GetCustomAttributesData());
                var attributesData = settingsType.GetCustomAttributesData().Concat(inheritedAttributesData).ToArray();
                for (int i = 0; i < attributes.Length; i++)
                {
                    string attributeText = CommonAttributeTranslator.Translate(attributes[i], attributesData[i], settingsType);
                    if (attributeText != null)
                        result.Add(attributeText);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        public virtual IEnumerable<string> GetAttributesTextFor(FieldInfo field, Usage defaultUsage, ParsingPolicyAttribute[] parsingPolicies)
        {
            var result = new List<string>();
            var fieldType = field.FieldType;
            string usageAttribute = null;
            bool isForcedUsage = defaultUsage == Usage.ForceRequired || defaultUsage == Usage.ForceOptional;
            if (isForcedUsage)
            {
                if (defaultUsage == Usage.ForceRequired) usageAttribute = AttributeBuilder.GetTextFor<RequiredAttribute>();
                else if (defaultUsage == Usage.ForceOptional) usageAttribute = AttributeBuilder.GetTextFor<OptionalAttribute>();
            }
            else if (fieldType.IsValueType && Nullable.GetUnderlyingType(field.FieldType) == null)
            {
                usageAttribute = AttributeBuilder.GetTextFor<RequiredAttribute>();
            }
            else if (Nullable.GetUnderlyingType(field.FieldType) != null)
            {
                usageAttribute = AttributeBuilder.GetTextFor<OptionalAttribute>();
            }
            else if (!field.IsDefined<RequiredAttribute>() && !field.IsDefined<OptionalAttribute>())
            {
                if (defaultUsage == Usage.Required) usageAttribute = AttributeBuilder.GetTextFor<RequiredAttribute>();
                else if (defaultUsage == Usage.Optional) usageAttribute = AttributeBuilder.GetTextFor<OptionalAttribute>();
            }
            if (usageAttribute != null)
                result.Add(usageAttribute);

            var attributes = field.GetCustomAttributes().ToArray();
            var attributesData = field.GetCustomAttributesData();
            for (int i = 0; i < attributes.Length; i++)
            {
                // Skip any usage attributes, if field usage is forced by the calling method.
                bool isUsageAttribute = attributes[i] is OptionalAttribute || attributes[i] is RequiredAttribute;
                if (isForcedUsage && isUsageAttribute) continue;
                // Get c# compatible attribute text.
                string attributeText = CommonAttributeTranslator.Translate(attributes[i], attributesData[i], field);
                if (attributeText != null)
                    result.Add(attributeText);
            }
            return result;
        }
    }
}
