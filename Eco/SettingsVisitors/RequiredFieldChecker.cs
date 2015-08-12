using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
    public class RequiredFieldChecker : IRefinedSettingsVisitor
    {
        public bool IsReversable { get { return true; } }

        public void Visit(string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            bool isRequiredField = refinedSettingsField.IsDefined<RequiredAttribute>() || refinedSettingsField.IsDefined<RequiredAttribute>();
            if (isRequiredField && refinedSettingsField.GetValue(refinedSettings) == null)
                throw new ConfigurationException("Missing required field '{0}'", fieldPath);
        }
    }
}
