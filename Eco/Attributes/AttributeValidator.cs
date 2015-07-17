using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;

namespace Eco
{
	static class AttributeValidator
	{
		public static void CheckAttributesCompatibility(FieldInfo attributeContext, HashSet<Type> incompatibleAttributeTypes)
		{
			if (attributeContext.GetCustomAttributes().Any(a => incompatibleAttributeTypes.Contains(a.GetType())))
			{
				throw new ConfigurationException(
					"Incomatible attributes detected: type={0}, field={1}, attributes={2}",
					attributeContext.DeclaringType.Name,
					attributeContext.Name,
					attributeContext.GetCustomAttributes()
						.Where(a => a.IsEcoAttribute())
						.Select(a => a.GetType().Name)
						.CommaWhiteSpaceSeparated()
				);
            }
		}
	}
}
