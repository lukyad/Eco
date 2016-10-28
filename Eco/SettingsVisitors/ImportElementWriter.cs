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
    /// Given a location of an imported file,
    /// writes all elements from the `data` field to that file using the specified serializer.
    /// </summary>
    public class ImportElementWriter : ImportElementProcessor
    {
        protected override void ProcessImportElement(object importElem)
        {
            // Skip readonly files.
            if (import.IsReadOnly(importElem)) return;
            // Create full path to the file (if doesn't exist)
            string filePath = include.GetFile(importElem);
            string dir = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(dir);
            // Write settings to the file.
            Type includedSettingsType = include.GetDataType(importElem);
            using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(fileStream))
            {
                var serializer = (ISerializer)Activator.CreateInstance(import.GetSerializerType(importElem));
                object settings = import.GetData(importElem);
                serializer.Serialize(settings, writer);
            }
        }
    }
}
