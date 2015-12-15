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
    /// Given a location of an included configuration file,
    /// writes corresponding settings to that file.
    /// </summary>
    public class IncludeElementWriter : IncludeElementProcessor
    {
        public IncludeElementWriter(SettingsManager context)
            : base(context)
        {
        }

        protected override void ProcessIncludeElement(object includeElem)
        {
            // Skip readonly files.
            if (include.IsReadOnly(includeElem)) return;
            // Create full path to the file (if doesn't exist)
            // If configuration file specifies a relative file path, then it's combined with the current working dir.
            string filePath = include.GetFile(includeElem);
            string dir = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(dir);
            // Write settings to the file.
            Type includedSettingsType = include.GetDataType(includeElem);
            using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(fileStream))
            {
                object settings = include.GetDataType(includeElem);
                this.Context.Serializer.Serialize(settings, writer);
            }
        }
    }
}
