using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    public class RequiredAttributesAttribute : Attribute
    {
        public RequiredAttributesAttribute(params Type[] attributeTypes)
        {
            if (!attributeTypes.All(t => t.IsSubclassOf(typeof(Attribute))))
                throw new ConfigurationException("'{0}' is not an attribute type", attributeTypes.First(t => !t.IsSubclassOf(typeof(Attribute))).FullName);
            AttributeTypes = attributeTypes;
        }

        public Type[] AttributeTypes { get; private set; }
    }
}
