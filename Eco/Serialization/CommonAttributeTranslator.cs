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
    static class CommonAttributeTranslator
    {
        public static string Translate(Attribute a, CustomAttributeData d, Type context)
        {
            if (a is EcoElementAttribute)
                return TranslateEcoElementAttribute((EcoElementAttribute)a);
            else
                return d.ToString();
        }

        public static string Translate(Attribute a, CustomAttributeData d, FieldInfo context)
        {
            var ecoAttr = a as EcoFieldAttribute;
            if (ecoAttr != null && !ecoAttr.ApplyToGeneratedClass) return null;

            if (a is KnownTypesAttribute)
                return TranslateKnownTypesAttribute((KnownTypesAttribute)a);
            else if (a is DefaultAttribute)
                return TranslateDefaultAttribute((DefaultAttribute)a);
            else
                return d.ToString();
        }

        static string TranslateKnownTypesAttribute(KnownTypesAttribute a)
        {
            var translation = new StringBuilder();
            foreach (var type in a.KnownTypes)
            {
                string attrText =
                    new AttributeBuilder(typeof(KnownTypesAttribute).FullName)
                    .AddTypeParam(type.GetNonGenericName())
                    .ToString();
                translation.AppendLine(attrText);
            }
            return translation.ToString();
        }

        static string TranslateDefaultAttribute(DefaultAttribute a)
        {
            bool isBooleanDefault = a.Value != null && a.Value.GetType() == typeof(bool);
            bool isStringDefault = a.Value != null && a.Value.GetType() == typeof(string);
            bool isEnumDefault = a.Value != null && a.Value.GetType().IsEnum;
            string defaultValue = a.Value.ToString();
            if (isBooleanDefault) defaultValue = defaultValue.ToLower();
            else if (isEnumDefault) defaultValue = $"{a.Value.GetType().Name}.{a.Value}";
            if (isStringDefault)
            {
                return
                     new AttributeBuilder(typeof(DefaultAttribute).FullName)
                     .AddStringParam(defaultValue)
                     .ToString();
            }
            else
            {
                return
                    new AttributeBuilder(typeof(DefaultAttribute).FullName)
                    .AddParam(defaultValue)
                    .ToString();
            }
        }

        static string TranslateEcoElementAttribute(EcoElementAttribute a)
        {
            return
                new AttributeBuilder(typeof(EcoElementAttribute).FullName)
                .AddTypeParam(a.ElementType.GetFullCsharpCompatibleName())
                .ToString();
        }
    }
}
