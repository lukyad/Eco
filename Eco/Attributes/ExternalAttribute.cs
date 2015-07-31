using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Eco.Elements;
using Eco.Extensions;

namespace Eco
{
    public class ExternalAttribute : FieldMutatorAttribute
    {
        class SampleType { }

        [ExternalSettingsType(typeof(SampleType))]
        readonly static object _attributePrototype = null;

        public ExternalAttribute()
            : base(GetRawSettingsFieldType, GetRawSettingsFieldAttributeText, GetRawSettingsFieldValue, SetRawSettingsFieldValueValue)
        {
        }

        public void ValidateContext(FieldInfo context)
        {
        }

        static new Type GetRawSettingsFieldType(FieldInfo refinedSettingsField)
        {
            return typeof(include);
        }

        static new string GetRawSettingsFieldAttributeText(FieldInfo refinedSettingsField)
        {
            return 
                typeof(ExternalAttribute)
                .GetField(nameof(_attributePrototype), BindingFlags.Static | BindingFlags.NonPublic)
                .GetCustomAttributesData()
                .First(d => d.AttributeType == typeof(ExternalSettingsTypeAttribute))
                .ToString()
                .Replace(typeof(SampleType).FullName, refinedSettingsField.FieldType.Name);
        }

        static new object GetRawSettingsFieldValue(FieldInfo rawSettingsField, object rawSettings)
        {
            include includePrototype;
            object includeElement = rawSettingsField.GetValue(rawSettings);
            if (includeElement == null) return null;

            string fileName = (string)includeElement.GetFieldValue(nameof(includePrototype.file));
            if (!File.Exists(fileName)) throw new ConfigurationException("Configuration file '{0}' doesn't exist", fileName);

            Type externalSettingsType = rawSettingsField.GetCustomAttribute<ExternalSettingsTypeAttribute>().Type;
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return Settings.DefaultManager.Serializer.Deserialize(externalSettingsType, fileStream);
        }

        static void SetRawSettingsFieldValueValue(FieldInfo rawSettingsField, object rawSettings, object nonMutatedRawSettingsValue)
        {
            // do nothing
        }
    }
}
