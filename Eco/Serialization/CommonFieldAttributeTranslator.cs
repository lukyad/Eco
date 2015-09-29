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
    static class CommonFieldAttributeTranslator
    {
        public static string Translate(Attribute a, CustomAttributeData d, FieldInfo context)
        {
            var mutator = a as FieldMutatorAttribute;
            if (mutator != null)
                return mutator.GetRawSettingsFieldAttributeText(context);
            else
                return d.ToString();
        }
    }
}
