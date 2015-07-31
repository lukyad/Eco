using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Reflection;
using Eco.Extensions;

namespace Eco.Serialization.Xml
{
    static class XmlFieldAttributeTranslator
    {
        class SampleType { }
        const string StringParam = "StringParam";

        [XmlAttribute]
        [XmlElement]
        static readonly object _attributesWithNoParams = null;

        [XmlElement(StringParam)]
        [XmlArrayItem(StringParam)]
        static readonly object _attributesWithOneStringParam = null;

        [XmlArrayItem(typeof(SampleType))]
        [XmlElement(typeof(SampleType))]
        static readonly object _attributesWithOneTypeParam = null;

        public static string GetTextFor<TAttribute>()
        {
            return
                typeof(XmlFieldAttributeTranslator).GetField(nameof(_attributesWithNoParams), BindingFlags.NonPublic | BindingFlags.Static)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == typeof(TAttribute))
                .ToString();
        }

        public static string GetTextFor<TAttribute>(string attributeParam)
        {
            return
                typeof(XmlFieldAttributeTranslator).GetField(nameof(_attributesWithOneStringParam), BindingFlags.NonPublic | BindingFlags.Static)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == typeof(TAttribute))
                .ToString()
                .Replace(StringParam, attributeParam);
        }

        public static string GetTextFor(Type attributeWithOneTypeParam, string typeName)
        {
            return
                typeof(XmlFieldAttributeTranslator).GetField(nameof(_attributesWithOneTypeParam), BindingFlags.NonPublic | BindingFlags.Static)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == attributeWithOneTypeParam)
                .ToString()
                .Replace(typeof(SampleType).FullName, typeName);
        }

        public static string Translate(Attribute a, CustomAttributeData d, FieldInfo context)
        {
            if (a is InlineAttribute) return TranslateInlineAttribute(context);
            if (a is ItemNameAttribute) return TranslateItemNameAttribute((ItemNameAttribute)a);
            else return null;
        }

        static string TranslateInlineAttribute(FieldInfo context)
        {
            if (context.IsPolimorphic()) return null;
            return GetTextFor<XmlElementAttribute>(context.FieldType.GetElementType().Name);
        }

        static string TranslateItemNameAttribute(ItemNameAttribute a)
        {
            return GetTextFor<XmlArrayItemAttribute>(a.Name);
        }
    }
}
