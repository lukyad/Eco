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
    public class XmlAttributesGenerator : CommonAttributesGenerator
    {
        public static string GetXmlNamesapceForRootType(Type rootSettingsType)
        {
            return rootSettingsType.Name + ".xsd";
        }

        public override IEnumerable<string> GetAttributesTextFor(Type settingsType)
        {
            var attributesText = base.GetAttributesTextFor(settingsType);
            if (settingsType.IsDefined<RootAttribute>())
                attributesText = attributesText.Append(XmlClassAttributeTranslator.GetTextFor<XmlRootAttribute>(GetXmlNamesapceForRootType(settingsType)));

            return attributesText;
        }

        public override IEnumerable<string> GetAttributesTextFor(FieldInfo field, Usage defaultUsage)
        {
            var res = new List<string>(base.GetAttributesTextFor(field, defaultUsage));

            var fieldType = field.FieldType;
            if (field.IsPolimorphic() && !field.IsDefined<RefAttribute>() && !field.IsDefined<FieldMutatorAttribute>())
            {
                Type attributeType = fieldType.IsArray || field.IsDefined<InlineAttribute>() ? typeof(XmlArrayItemAttribute) : typeof(XmlElementAttribute);
                foreach (var t in field.GetKnownSerializableTypes())
                    res.Add(XmlFieldAttributeTranslator.GetTextFor(attributeType, t.GetNonGenericName()));
            }

            var attributes = field.GetCustomAttributes().ToArray();
            var attributesData = field.GetCustomAttributesData();
            for (int i = 0; i < attributes.Length; i++)
                res.Add(XmlFieldAttributeTranslator.Translate(attributes[i], attributesData[i], field));

            if (field.GetRawFieldType().IsSimple())
                res.Add(XmlFieldAttributeTranslator.GetTextFor<XmlAttributeAttribute>());

            return res;
        }
    }
}
