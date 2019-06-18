using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Eco.Serialization;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Handles Eco.import<> element. Given a location of a file to be imported,
    /// reads all data elements from it using the specified by the import<> element serializer.
    /// </summary>
    public class ImportElementProcessor : SettingsVisitorBase
    {
        readonly SettingsManager _context;

        public ImportElementProcessor(SettingsManager context)
             : base(isReversable: true)
        {
            _context = context;
        }

        public override void Visit(string settingsNamesapce, string settingsPath, object rawSettings)
        {
            if (!rawSettings.IsEcoElementOfGenericType(typeof(import<,>))) return;
            var importElem = rawSettings;

            Type importedSettingsType = import.GetDataType(importElem);
            // Pls note, that Eco validates Importer's ctor in the import<,> ctor of the import element.
            var importer = (IImporter)Activator.CreateInstance(import.GetImporterType(importElem), importElem);
            object importedSettings = importer.Import();
            if (importedSettings != null && importedSettings.GetType() != importedSettingsType)
                throw new ConfigurationException("Invalid import. Expected data of type `{0}`, but got `{1}`", importedSettingsType.FullName, importedSettings.GetType().FullName);

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

        class init
        {
            public object data;
        }
    }
}
