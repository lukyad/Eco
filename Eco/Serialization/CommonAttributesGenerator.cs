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
            var result = new List<string>();
            var attributes = settingsType.GetCustomAttributes().ToArray();
            var attributesData = settingsType.GetCustomAttributesData();
            for (int i = 0; i < attributes.Length; i++)
            {
                string attributeText = CommonAttributeTranslator.Translate(attributes[i], attributesData[i], settingsType);
                if (attributeText != null)
                    result.Add(attributeText);
            }
            return result;
        }

        public virtual IEnumerable<string> GetAttributesTextFor(FieldInfo field, Usage defaultUsage, ParsingPolicyAttribute[] parsingPolicies)
        {
            var result = new List<string>();
            var fieldType = field.FieldType;
            string usageAttribute = null;
            if (fieldType.IsSimple() && fieldType != typeof(string))
            {
                usageAttribute = AttributeBuilder.GetTextFor<RequiredAttribute>();
            }
            else if (Nullable.GetUnderlyingType(field.FieldType) != null)
            {
                usageAttribute = AttributeBuilder.GetTextFor<OptionalAttribute>();
            }
            else if (!field.IsDefined<RequiredAttribute>() && !field.IsDefined<OptionalAttribute>())
            {
                usageAttribute = defaultUsage == Usage.Required ?
                    AttributeBuilder.GetTextFor<RequiredAttribute>() :
                    AttributeBuilder.GetTextFor<OptionalAttribute>();
            }
            if (usageAttribute != null)
                result.Add(usageAttribute);

            var attributes = field.GetCustomAttributes().ToArray();
            var attributesData = field.GetCustomAttributesData();
            for (int i = 0; i < attributes.Length; i++)
            {
                string attributeText = CommonAttributeTranslator.Translate(attributes[i], attributesData[i], field);
                if (attributeText != null)
                    result.Add(attributeText);
            }
            return result;
        }
    }
}
