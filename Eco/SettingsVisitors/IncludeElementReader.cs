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
        public IncludeElementReader(SettingsManager context)
            : base(context)
        {
        }

        protected override void ProcessIncludeElement(object includeElem)
        {
            string filePath = include.GetFile(includeElem);
            if (!File.Exists(filePath)) throw new ConfigurationException("Configuration file '{0}' doesn't exist.", filePath);

            Type includedSettingsType = include.GetDataType(includeElem);
            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                // Note, that we skip visitor initialization here, as this should be done
                // only one per the root configuration file. (i.e. by this moment, all visitors have been initialized already)
                object includedSettings = this.Context.ReadRawSettings(includedSettingsType, reader, initializeVisitors: false);
                include.SetData(includeElem, includedSettings);
            }
        }
    }
}
