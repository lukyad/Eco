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
        protected override void ProcessIncludeElement(include includeElem)
        {
            string fileName = includeElem.file;
            string dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir)) throw new ConfigurationException("Directory '{0}' doesn't exist.", dir);

            Type includedSettingsType = GetIncludedDataType(includeElem);
            using (var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(fileStream))
            {
                object settings = GetIncludedData(includeElem);
                Settings.DefaultManager.Serializer.Serialize(settings, writer);
            }
        }
    }
}
