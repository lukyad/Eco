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
        public void Visit(string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.FieldType == typeof(string) && !refinedSettingsField.IsDefined<SealedAttribute>())
            {
                string expandedStr = Environment.ExpandEnvironmentVariables((string)refinedSettingsField.GetValue(refinedSettings));
                refinedSettingsField.SetValue(refinedSettings, expandedStr);
            }
        }
    }
}
