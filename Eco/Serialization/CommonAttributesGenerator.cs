using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.Serialization.Xml
{
    public class CommonAttributesGenerator : ISerializationAttributesGenerator
    {
        public virtual IEnumerable<string> GetAttributesTextFor(Type settingsType)
        {
            return Enumerable.Empty<string>();
        }

        public virtual IEnumerable<string> GetAttributesTextFor(FieldInfo field, Usage defaultUsage)
        {
            var res = new List<string>();

            var fieldType = field.FieldType;
            string usageAttribute = null;
            if (fieldType.IsSimple() && fieldType != typeof(string))
            {
                usageAttribute = CommonFieldAttributeTranslator.GetTextFor<RequiredAttribute>();
            }
            else if (Nullable.GetUnderlyingType(field.FieldType) != null)
            {
                usageAttribute = CommonFieldAttributeTranslator.GetTextFor<OptionalAttribute>();
            }
            else if (!field.IsDefined<RequiredAttribute>() && !field.IsDefined<OptionalAttribute>())
            {
                usageAttribute = defaultUsage == Usage.Required ?
                    CommonFieldAttributeTranslator.GetTextFor<RequiredAttribute>() :
                    CommonFieldAttributeTranslator.GetTextFor<OptionalAttribute>();
            }
            if (usageAttribute != null)
                res.Add(usageAttribute);

            var attributes = field.GetCustomAttributes().ToArray();
            var attributesData = field.GetCustomAttributesData();
            for (int i = 0; i < attributes.Length; i++)
            {
                string attributeText = CommonFieldAttributeTranslator.Translate(attributes[i], attributesData[i], field);
                if (attributeText != null)
                    res.Add(attributeText);
            }

            return res;
        }
    }
}
