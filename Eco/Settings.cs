using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Eco.Serialization.Xml;

namespace Eco
{
    /// <summary>
    /// Provide access to basic load/save settins functionality.
    /// If you need more controll, check SettingsManager class.
    /// </summary>
    public static class Settings
    {
        // Character used to separate IDs in the reference fields (ie string fields marked with the Ref attribute).
        public const char IdSeparator = ',';

        public readonly static object Null = new object();

        public const string NullId = "null";

        // Default SettingsManager used to load/save settings.
        public static SettingsManager DefaultManager = new SettingsManager(new XmlSerializer(), new XmlAttributesGenerator());

        /// <summary>
        /// Loads settings of the specified type from a configuration file in the current working directory.
        /// Name of the file is defined as typeof(T).Name + ".config"
        /// </summary>
        public static T Load<T>()
        {
            return DefaultManager.Load<T>();
        }

        /// <summary>
        /// Loads settings from the specified configuration file.
        /// </summary>
        public static T Load<T>(string fileName)
        {
            return DefaultManager.Load<T>(fileName);
        }

        /// <summary>
        /// Saves settings to the configuration file in the current working directory.
        /// Name of the file is defined as typeof(T).Name + ".config"
        /// </summary>
        public static void Save(object settings)
        {
            DefaultManager.Save(settings);
        }

        /// <summary>
        /// Saves settings to the specified configuration file.
        /// </summary>
        public static void Save(object settings, string fileName)
        {
            DefaultManager.Save(settings, fileName);
        }

        /// <summary>
        /// Read settins of the specified type from a stream.
        /// </summary>
        public static T Read<T>(Stream stream)
        {
            return DefaultManager.Read<T>(stream);
        }

        /// <summary>
        /// Read settins of the specified type from the specified TextReader.
        /// </summary>
        public static T Read<T>(TextReader reader)
        {
            return DefaultManager.Read<T>(reader);
        }

        /// <summary>
        /// Write settins to a stream.
        /// </summary>
        public static void Write(object settings, Stream stream)
        {
            DefaultManager.Write(settings, stream);
        }

        /// <summary>
        /// Write settins using the specified TextWriter.
        /// </summary>
        public static void Write(object settings, TextWriter stream)
        {
            DefaultManager.Write(settings, stream);
        }

        /// <summary>
        /// Given a root settings object, enumerate all child settings objects recursive.
        /// </summary>
        public static IEnumerable<object> Enumerate(object root)
        {
            return SettingsManager.EnumerateSettings(root);
        }
    }
}
