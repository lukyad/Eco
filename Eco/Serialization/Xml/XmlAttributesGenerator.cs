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

            return attributesText;
        }

        public override IEnumerable<string> GetAttributesTextFor(FieldInfo field, Usage defaultUsage, ParsingPolicyAttribute[] parsingPolicies)
        {
            var res = new List<string>(base.GetAttributesTextFor(field, defaultUsage, parsingPolicies));

            var fieldType = field.FieldType;
            var renameRule = field.GetCustomAttribute<RenameAttribute>();

            if (!field.IsDefined<RefAttribute>() && !field.IsDefined<FieldMutatorAttribute>())
            {
                if (field.IsPolymorphic())
                {
                    Type attributeType = !fieldType.IsArray || field.IsDefined<InlineAttribute>() ? typeof(XmlElementAttribute) : typeof(XmlArrayItemAttribute);

                    foreach (var t in field.GetKnownSerializableTypes())
                        res.Add(GetItemAttributeText(attributeType, t, renameRule));
                }
                else if (field.FieldType.IsArray)
                {
                    Type attributeType = field.IsDefined<InlineAttribute>() ? typeof(XmlElementAttribute) : typeof(XmlArrayItemAttribute);
                    Type itemTypeName = field.FieldType.GetElementType();
                    res.Add(GetItemAttributeText(attributeType, itemTypeName, renameRule));
                }
            }

            if (field.GetRawFieldType(parsingPolicies).IsSimple())
                res.Add(AttributeBuilder.GetTextFor<XmlAttributeAttribute>());

            return res.Where(a => a != null);
        }

        static string GetItemAttributeText(Type attributeType, Type itemType, RenameAttribute renameRule)
        {
            string originalItemTypeName = itemType.GetNonGenericName();
            string xmlItemTypeName = renameRule != null ? renameRule.Rename(originalItemTypeName) : originalItemTypeName;
            return 
                new AttributeBuilder(attributeType.FullName)
                .AddStringParam(xmlItemTypeName)
                .AddTypeParam(originalItemTypeName)
                .ToString();
        }
    }
}
