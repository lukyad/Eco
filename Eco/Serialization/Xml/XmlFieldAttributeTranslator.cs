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
        const string stringParam = "stringParam";

        [XmlAttribute]
        [XmlElement]
        [Optional]
        [Required]
        static readonly string attributesWithNoParams = null;

        [XmlElement(stringParam)]
        [XmlArrayItem(stringParam)]
        static readonly string attributesWithOneStringParam = null;

        [XmlArrayItem(typeof(SampleType))]
        [XmlElement(typeof(SampleType))]
        static readonly string attributesWithOneTypeParam = null;

        public static string GetTextFor<TAttribute>()
        {
            return
                typeof(XmlFieldAttributeTranslator).GetField("attributesWithNoParams", BindingFlags.NonPublic | BindingFlags.Static)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == typeof(TAttribute))
                .ToString();
        }

        public static string GetTextFor<TAttribute>(string attributeParam)
        {
            return
                typeof(XmlFieldAttributeTranslator).GetField("attributesWithOneStringParam", BindingFlags.NonPublic | BindingFlags.Static)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == typeof(TAttribute))
                .ToString()
                .Replace(stringParam, attributeParam);
        }

        public static string GetTextFor<TAttribute>(Type dummy, string typeName)
        {
            return
                typeof(XmlFieldAttributeTranslator).GetField("attributesWithOneTypeParam", BindingFlags.NonPublic | BindingFlags.Static)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == typeof(TAttribute))
                .ToString()
                .Replace(typeof(SampleType).FullName, typeName);
        }



        public static string Translate(Attribute a, CustomAttributeData d, FieldInfo context, string settingsNamespace)
        {
            if (a is InlineAttribute) return TranslateInlineAttribute(context, settingsNamespace);
            if (a is ChoiceAttribute) return TranslateChoiceAttribute(context, settingsNamespace);
            if (a is ItemNameAttribute) return TranslateItemNameAttribute((ItemNameAttribute)a);
            if (a is KnownTypesAttribute) return TranslateKnownTypesAttribute((KnownTypesAttribute)a, d, context, settingsNamespace);
            return d.ToString();
        }

        static string TranslateInlineAttribute(FieldInfo context, string settingsNamespace)
        {
            if (context.IsDefined(typeof(KnownTypesAttribute))) return null;

            var knownTypes = context.GetSerializableTypes();
            if (knownTypes.Count() == 1)
            {
                return GetTextFor<XmlElementAttribute>(context.FieldType.GetElementType().Name);
            }
            else
            {
                return
                    knownTypes
                    .Select(t => XmlFieldAttributeTranslator.GetTextFor<XmlElementAttribute>(default(Type), t.GetFriendlyName(settingsNamespace)))
                    .SeparatedBy(Environment.NewLine);
            }
        }

        //static string TranslateInlineAttribute(FieldInfo context, string settingsNamespace)
        //{
        //    if (context.IsDefined(typeof(KnownTypesAttribute))) return null;
        //    return GetTextFor<XmlElementAttribute>(context.FieldType.GetElementType().Name);
        //}

        static string TranslateChoiceAttribute(FieldInfo context, string settingsNamespace)
        {
            if (context.IsDefined(typeof(KnownTypesAttribute))) return null;
            return
                context.GetSerializableTypes()
                .Select(t => XmlFieldAttributeTranslator.GetTextFor<XmlElementAttribute>(default(Type), t.GetFriendlyName(settingsNamespace)))
                .SeparatedBy(Environment.NewLine);
        }

        static string TranslateItemNameAttribute(ItemNameAttribute a)
        {
            return GetTextFor<XmlArrayItemAttribute>(a.Name);
        }

        static string TranslateKnownTypesAttribute(KnownTypesAttribute a, CustomAttributeData d, FieldInfo context, string settingsNamespace)
        {
            Func<Type, string> GetAttributeText;
            if (context.IsDefined<InlineAttribute>() || context.IsDefined<ChoiceAttribute>())
                GetAttributeText = t => GetTextFor<XmlElementAttribute>(t, t.GetFriendlyName(settingsNamespace));
            else
                GetAttributeText = t => GetTextFor<XmlArrayItemAttribute>(t, t.GetFriendlyName(settingsNamespace));

            return
                a.GetAllKnownTypes(context)
                .Where(t => !t.IsAbstract)
                .Select(t => GetAttributeText(t))
                .Append(d.ToString())
                .SeparatedBy(Environment.NewLine);
        }
    }
}
