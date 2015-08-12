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
    /// <summary>
    /// Instructs serializer to load value for the given field from another file 
    /// by using SettingsManager.Load method with the specified file format.
    /// 
    /// Usage: 
    /// Can be applied to a field of a settings type only. Type of the 'extrnal' field must be marked with the Root attribute.
    /// 
    /// Comatibility: 
    /// Compatible only with the Doc, Required and Optional attributes and incompatible with all other settings attributes.
    /// </summary>
    public class ExternalAttribute : FieldMutatorAttribute
    {
        class SampleType { }

        [ExternalSettingsType(typeof(SampleType))]
        readonly static object _attributePrototype = null;

        public ExternalAttribute()
            : base(GetRawSettingsFieldType, GetRawSettingsFieldAttributeText, GetRawSettingsFieldValue, SetRawSettingsFieldValueValue)
        {
        }

        public override void ValidateContext(FieldInfo context)
        {
            //throw new NotImplementedException();
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
