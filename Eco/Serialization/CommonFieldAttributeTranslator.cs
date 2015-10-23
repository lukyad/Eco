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
    static class CommonFieldAttributeTranslator
    {
        public static string Translate(Attribute a, CustomAttributeData d, FieldInfo context)
        {
            if (a is OptionalAttribute || a is RequiredAttribute)
                return d.ToString();
            else if (a is KnownTypesAttribute)
                return TranslateKnownTypesAttribute((KnownTypesAttribute)a, context);
            else if (a is DocAttribute)
                return d.ToString();
            else if (a is EcoAttribute)
                return null;
            else
                return d.ToString();
        }

        static string TranslateKnownTypesAttribute(KnownTypesAttribute a, FieldInfo context)
        {
            var translation = new StringBuilder();
            foreach (var type in a.GetKnownTypes(context))
            {
                string attrText = 
                    new AttributeBuilder(typeof(KnownTypesAttribute).FullName)
                    .AddTypeParam(type.Name)
                    .ToString();
                translation.AppendLine(attrText);
            }
            return translation.ToString();
        }
    }
}
