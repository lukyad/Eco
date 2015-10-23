using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Handles Eco.include and Eco.include<T> elements.
    /// Given a location of a configuration file to be included,
    /// reads and merges it with the main configuration settings.
    /// </summary>
    public class IncludeElementReader : IncludeElementProcessor
    {
        protected override void ProcessIncludeElement(object includeElem)
        {
            include<object> includeElemPrototype;
            string fileName = (string)includeElem.GetFieldValue(nameof(includeElemPrototype.file));
            if (!File.Exists(fileName)) throw new ConfigurationException("Configuration file '{0}' doesn't exist.", fileName);

            string settingsFieldName = nameof(includeElemPrototype.data);
            Type includedSettingsType = (Type)includeElem.GetType().GetField(settingsFieldName).FieldType;
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                object includedSettings = Settings.DefaultManager.Serializer.Deserialize(includedSettingsType, reader);
                includeElem.SetFieldValue(settingsFieldName, includedSettings);
            }
        }
    }
}
