using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Used by the Eco library to set default raw setting values.
    /// </summary>
    public class DefaultValueSetter : ISettingsVisitor
    {
        // Changes made by the DefaultValueSetter are not revocable.
        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRawSettingsType) { }

        public void Visit(string settingsNamespace, string fieldPath, object rawSettings) { }

        public void Visit(string settingsNamespace, string fieldPath, FieldInfo rawSettingsField, object rawSettings)
        {
            var defaultAttr = rawSettingsField.GetCustomAttribute<DefaultAttribute>();
            if (defaultAttr != null)
            {
                if (defaultAttr.Value != null && !rawSettingsField.FieldType.IsAssignableFrom(defaultAttr.Value.GetType()))
                    throw new ConfigurationException("Invalud default field value for {0}.{1}: '{2}'.", rawSettingsField.DeclaringType.Name, rawSettingsField.Name, defaultAttr.Value);

                object targetValue = rawSettingsField.GetValue(rawSettings);
                if (targetValue == null)
                    rawSettingsField.SetValue(rawSettings, defaultAttr.Value);
            }
        }
    }
}
