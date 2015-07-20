using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
	// TODO implement me
	class RequiredFieldChecker : IFieldVisitor
	{
		public void Visit(string fieldPath, FieldInfo sourceField, object sourceSettings, FieldInfo targetField, object targetSettings)
		{
			bool isRequiredField = sourceField.IsDefined<RequiredAttribute>() || targetField.IsDefined<RequiredAttribute>();
			if (isRequiredField && sourceField.GetValue(sourceSettings) == null)
				throw new ConfigurationException("Missing required field '{0}'", fieldPath);
		}
	}
}
