using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Eco.Serialization;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Handles Eco.import<> element.
    /// Given a location of a file to be imported,
    /// reads all data elements from it using the specified by the import<> element serializer.
    /// </summary>
    public class ImportElementReader : ImportElementProcessor
    {
        readonly SettingsManager _context;

        public ImportElementReader(SettingsManager context)
        {
            _context = context;
        }

        protected override void ProcessImportElement(string settingsNamesapce, string settingsPath, object importElem)
        {
            string filePath = import.GetFile(importElem);
            if (!File.Exists(filePath)) throw new ConfigurationException("Imported file `{0}` doesn't exist.", filePath);

            Type importedSettingsType = import.GetDataType(importElem);
            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                var serializer = (ISerializer)Activator.CreateInstance(import.GetSerializerType(importElem));
                object importedSettings = serializer.Deserialize(importedSettingsType, reader);
                if (importedSettings != null && importedSettings.GetType() != importedSettingsType)
                    throw new ConfigurationException("Invalid import: `{0}`. Expected data of type `{1}`, but got `{2}`", filePath, importedSettingsType.FullName, importedSettings.GetType().FullName);

                // Initialize importedSettings.
                // Pls, note that we pass importedSettings to the SettingsManager as a field of the init class instance.
                // This is required, as SettingsManager.InitilizeRawSettings accepts only non-array objects as an input.
                _context.InitilizeRawSettings(
                    currentNamespace: settingsNamesapce,
                    currentSettingsPath: settingsPath,
                    rawSettings: new init { data = importedSettings },
                    initializeVisitors: false);

                import.SetData(importElem, importedSettings);
            }
        }

        class init
        {
            public object data;
        }
    }
}
