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
        protected override void ProcessIncludeElement(include includeElem)
        {
            string fileName = includeElem.file;
            if (!File.Exists(fileName)) throw new ConfigurationException("Configuration file '{0}' doesn't exist.", fileName);

            Type includedSettingsType = GetIncludedDataType(includeElem);
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                object includedSettings = Settings.DefaultManager.Serializer.Deserialize(includedSettingsType, reader);
                SetIncludedData(includeElem, includedSettings);
            }
        }
    }
}
