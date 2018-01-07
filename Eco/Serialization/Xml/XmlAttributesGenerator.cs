using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eco.Extensions;
using Eco.CodeBuilder;

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
            {
                string xmlRootAttribute =
                    new AttributeBuilder(typeof(XmlRootAttribute).FullName)
                    .AddStringParam("Namespace", GetXmlNamesapceForRootType(settingsType))
                    .ToString();
                attributesText = attributesText.Append(xmlRootAttribute);
            }

            string typeName = settingsType.GetCustomAttribute<NameAttribute>()?.Name ?? settingsType.Name;
            attributesText.Append(AttributeBuilder.GetTextFor<XmlTypeAttribute>(typeName));

            return attributesText;
        }

        public override IEnumerable<string> GetAttributesTextFor(FieldInfo field, Usage defaultUsage, ParsingPolicyAttribute[] parsingPolicies)
        {
            var res = new List<string>(base.GetAttributesTextFor(field, defaultUsage, parsingPolicies));

            var fieldType = field.FieldType;
            var renameRules = field.GetCustomAttributes<RenameAttribute>().ToArray();
            string fieldName = field.GetCustomAttribute<NameAttribute>()?.Name ?? field.Name;

            if (!field.IsDefined<RefAttribute>())
            {
                if (field.IsPolymorphic())
                {
                    Type attributeType = !fieldType.IsArray || field.IsDefined<InlineAttribute>() ? typeof(XmlElementAttribute) : typeof(XmlArrayItemAttribute);
                    foreach (var t in field.GetKnownSerializableTypes())
                        res.Add(GetItemAttributeText(attributeType, t, renameRules));
                }
                else if (
                    field.FieldType.IsArray && 
                    !field.IsDefined<ConverterAttribute>() && 
                    !field.IsDefined<ParserAttribute>() &&
                    !parsingPolicies.Any(p => p.CanParse(field.FieldType)))
                {
                    Type attributeType = field.IsDefined<InlineAttribute>() ? typeof(XmlElementAttribute) : typeof(XmlArrayItemAttribute);
                    Type itemTypeName = field.FieldType.GetElementType();
                    res.Add(GetItemAttributeText(attributeType, itemTypeName, renameRules));
                }
            }

            var rawFieldType = field.GetRawFieldType(parsingPolicies);
            if (rawFieldType.IsSimple())
            {
                res.Add(AttributeBuilder.GetTextFor<XmlAttributeAttribute>(fieldName));
            }
            else if (!res.Any(a => a.Contains(nameof(XmlElementAttribute))))
            {
                 if (rawFieldType.IsArray)
                    res.Add(AttributeBuilder.GetTextFor<XmlArrayAttribute>(fieldName));
                 else
                    res.Add(AttributeBuilder.GetTextFor<XmlElementAttribute>(fieldName));
            }
            


            if (field.IsDefined<HiddenAttribute>())
                res.Add(AttributeBuilder.GetTextFor<XmlIgnoreAttribute>());

            return res.Where(a => a != null);
        }

        static string GetItemAttributeText(Type attributeType, Type itemType, RenameAttribute[] renameRules)
        {
            string originalItemTypeName = itemType.GetNonGenericName();
            string xmlItemTypeName = originalItemTypeName;
            foreach (var renameRule in renameRules)
                xmlItemTypeName = renameRule.Rename(xmlItemTypeName);

            return 
                new AttributeBuilder(attributeType.FullName)
                .AddStringParam(xmlItemTypeName)
                .AddTypeParam(originalItemTypeName)
                .ToString();
        }
    }
}
