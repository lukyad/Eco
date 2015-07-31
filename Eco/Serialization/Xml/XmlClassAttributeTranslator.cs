using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eco.Serialization.Xml
{
    static class XmlClassAttributeTranslator
    {
        const string StringParam = "StringParam";

        [XmlRoot(Namespace = StringParam)]
        class AttributesWitnOneStringParam
        {
        }

        public static string GetTextFor<TAttribute>(string attributeParam)
        {
            return
                typeof(AttributesWitnOneStringParam)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == typeof(TAttribute))
                .ToString()
                .Replace(StringParam, attributeParam);
        }
    }
}
