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
    public class ImportElementProcessor : TwinSettingsVisitorBase
    {
        readonly SettingsManager _context;

        public ImportElementProcessor(SettingsManager context)
             : base(isReversable: true)
        {
            _context = context;
        }

        public override void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            if (!refinedSettings.IsEcoElementOfGenericType(typeof(import<,>))) return;
            var importElem = refinedSettings;

            Type importedSettingsType = import.GetDataType(importElem);
            // Pls note, that Eco validates Importer's ctor in the import<,> ctor of the import element.
            var importer = (IImporter)Activator.CreateInstance(import.GetImporterType(importElem), importElem);
            object importedData = importer.Import();
            if (importedData != null && importedData.GetType() != importedSettingsType)
                throw new ConfigurationException("Invalid import. Expected data of type `{0}`, but got `{1}`", importedSettingsType.FullName, importedData.GetType().FullName);

            import.SetData(refinedSettings, importedData);
        }

        class init
        {
            public object data;
        }
    }
}
