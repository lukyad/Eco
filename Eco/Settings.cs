using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Eco.Serialization.Xml;

namespace Eco
{
    public static class Settings
    {
        public const char IdSeparator = ',';

        public static SettingsManager DefaultManager = new SettingsManager(new XmlSerializer(), new XmlAttributesGenerator());


        public static T Load<T>()
        {
            return DefaultManager.Load<T>();
        }

        public static T Load<T>(string fileName)
        {
            return DefaultManager.Load<T>(fileName);
        }

        public static void Save(object settings)
        {
            DefaultManager.Save(settings);
        }

        public static void Save(object settings, string fileName)
        {
            DefaultManager.Save(settings, fileName);
        }

        public static T Read<T>(Stream stream)
        {
            return DefaultManager.Read<T>(stream);
        }

        public static void Write(object settings, Stream stream)
        {
            DefaultManager.Write(settings, stream);
        }
    }
}
