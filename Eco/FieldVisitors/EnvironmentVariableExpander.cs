using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
	class EnvironmentVariableExpander : IFieldVisitor
	{
		public void Visit(string fieldPath, FieldInfo sourceField, object sourceSettings, FieldInfo targetField, object targetSettings)
		{
			if (targetField.FieldType == typeof(string) && !targetField.IsDefined<SealedAttribute>())
			{
				string expandedStr = Environment.ExpandEnvironmentVariables((string)targetField.GetValue(targetSettings));
				targetField.SetValue(targetSettings, expandedStr);
			}
        }
	}
}
