using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Reflection;
using Eco.Extensions;
using Eco.CodeBuilder;

namespace Eco.Serialization.Xml
{
    static class XmlFieldAttributeTranslator
    {
        public static string Translate(Attribute a, CustomAttributeData d, FieldInfo context)
        {
            if (a is InlineAttribute) return TranslateInlineAttribute(context);
            if (a is ItemNameAttribute) return TranslateItemNameAttribute((ItemNameAttribute)a);
            else return null;
        }

        static string TranslateInlineAttribute(FieldInfo context)
        {
            if (context.IsPolimorphic()) return null;
            return
                new AttributeBuilder(typeof(XmlElementAttribute).FullName)
                .AddStringParam(context.FieldType.GetElementType().Name)
                .ToString();
        }

        static string TranslateItemNameAttribute(ItemNameAttribute a)
        {
            return AttributeBuilder.GetTextFor<XmlArrayItemAttribute>(a.Name);
        }
    }
}
