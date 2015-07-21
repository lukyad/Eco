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
        [Serializable]
        class AttributesWithNoParams
        {
        }

        [XmlRoot(Namespace = stringParam)]
        class AttributesWitnOneStringParam
        {

        }

        const string stringParam = "stringParam";

        public static string GetTextFor<TAttribute>()
        {
            return
                typeof(AttributesWithNoParams)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == typeof(TAttribute))
                .ToString();
        }

        public static string GetTextFor<TAttribute>(string attributeParam)
        {
            return
                typeof(AttributesWitnOneStringParam)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == typeof(TAttribute))
                .ToString()
                .Replace(stringParam, attributeParam);
        }
    }
}
