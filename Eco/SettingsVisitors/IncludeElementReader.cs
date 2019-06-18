using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Eco.Serialization;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Handles Eco.include and Eco.include<T> elements.
    /// Given a location of a configuration file to be included,
    /// reads and merges it with the main configuration settings.
    /// </summary>
    public class IncludeElementReader : IncludeElementProcessor
    {
        public IncludeElementReader(SettingsManager context)
            : base(context)
        {
        }

        protected override void ProcessIncludeElement(string settingsNamesapce, string settingsPath, object includeElem)
        {
            string filePath = include.GetFile(includeElem);
            if (!File.Exists(filePath))
            {
                if (!include.IsOptional(includeElem))
                    throw new ConfigurationException("Configuration file '{0}' doesn't exist.", filePath);
                return;
            }

            Type includedSettingsType = include.GetDataType(includeElem);
            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                var serializer = GetSerializer(includeElem);
                object includedSettings = serializer.Deserialize(includedSettingsType, reader);
                if (includedSettings != null && includedSettings.GetType() != includedSettingsType)
                    throw new ConfigurationException("Invalid include: `{0}`. Expected data of type `{1}`, but got `{2}`", filePath, includedSettingsType.FullName, includedSettings.GetType().FullName);

                // Initialize includedSettings.
                // Pls, note that we pass importedSettings to the SettingsManager as a field of the init class instance.
                // This is required, as SettingsManager.InitilizeRawSettings accepts only non-array objects as an input.
                this.Context.InitilizeRawSettings(
                    currentNamespace: settingsNamesapce,
                    currentSettingsPath: settingsPath,
                    rawSettings: new init { data = includedSettings },
                    initializeVisitors: false);

                include.SetData(includeElem, includedSettings);
            }
        }

        class init
        {
            public object data;
        }
    }
}
