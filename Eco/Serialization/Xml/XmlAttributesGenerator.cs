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
    public class XmlAttributesGenerator : ISerializationAttributesGenerator
    {
        public static string GetXmlNamesapceForRootType(Type rootSettingsType)
        {
            return rootSettingsType.Name + ".xsd";
        }

        public IEnumerable<string> GetAttributesTextForRootType(Type rootSettingsType, string schemaNamespace)
        {
            return
                rootSettingsType.GetCustomAttributesData()
                .Select(d => d.ToString())
                .Append(XmlClassAttributeTranslator.GetTextFor<SerializableAttribute>())
                .Append(XmlClassAttributeTranslator.GetTextFor<XmlRootAttribute>(schemaNamespace));
        }

        public IEnumerable<string> GetAttributesTextFor(Type settingsType, bool isRoot)
        {
            var attributesText =
                settingsType.GetCustomAttributesData()
                .Select(d => d.ToString())
                .Append(XmlClassAttributeTranslator.GetTextFor<SerializableAttribute>());

            if (isRoot) attributesText = attributesText.Append(XmlClassAttributeTranslator.GetTextFor<XmlRootAttribute>(GetXmlNamesapceForRootType(settingsType)));

            return attributesText;
        }

        public IEnumerable<string> GetAttributesTextFor(string settingsNamespace, FieldInfo settingsField, Usage defaultUsage)
        {
            var res = new List<string>();

            var attributes = settingsField.GetCustomAttributes().ToArray();
            var attributesData = settingsField.GetCustomAttributesData();
            for (int i = 0; i < attributes.Length; i++)
                res.Add(XmlFieldAttributeTranslator.Translate(attributes[i], attributesData[i], settingsField, settingsNamespace));

            var fieldType = settingsField.FieldType;
            if (fieldType.IsSettingsArrayType()
                && !settingsField.IsDefined<RefAttribute>()
                && !settingsField.IsDefined<KnownTypesAttribute>()
                && !settingsField.IsDefined<InlineAttribute>())
            {
                foreach (var t in settingsField.GetSerializableTypes())
                    res.Add(XmlFieldAttributeTranslator.GetTextFor<XmlArrayItemAttribute>(default(Type), t.GetFriendlyName(settingsNamespace)));
            }

            if (fieldType.IsSimple() ||
                Nullable.GetUnderlyingType(fieldType).IsSimple() ||
                settingsField.IsDefined<RefAttribute>() ||
                settingsField.IsDefined<ConverterAttribute>())
            {
                res.Add(XmlFieldAttributeTranslator.GetTextFor<XmlAttributeAttribute>());
            }

            string usageAttribute = null;
            if (settingsField.FieldType.IsSimple() && settingsField.FieldType.IsValueType)
            {
                usageAttribute = XmlFieldAttributeTranslator.GetTextFor<RequiredAttribute>();
            }
            else if (Nullable.GetUnderlyingType(settingsField.FieldType).IsSimple())
            {
                usageAttribute = XmlFieldAttributeTranslator.GetTextFor<OptionalAttribute>();
            }
            else if (!settingsField.IsDefined<RequiredAttribute>() && !settingsField.IsDefined<OptionalAttribute>())
            {
                usageAttribute = defaultUsage == Usage.Required ?
                    XmlFieldAttributeTranslator.GetTextFor<RequiredAttribute>() :
                    XmlFieldAttributeTranslator.GetTextFor<OptionalAttribute>();
            }
            if (usageAttribute != null)
                res.Add(usageAttribute);

            return res;
        }
    }
}
