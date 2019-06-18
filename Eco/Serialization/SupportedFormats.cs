using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;

namespace Eco.Serialization
{
    // Automatically registers all Serializer classes marked with the SettingsSerializerAttribute
    public static class SupportedFormats
    {
        readonly static Dictionary<string, Type> _serializerTypes = new Dictionary<string, Type>();

        public const string Csv = "csv";
        public const string Xml = "xml";

        static SupportedFormats()
        {
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
            { 
                if (t.IsDefined(typeof(SettingsSerializerAttribute)))
                    Add(format: t.GetCustomAttribute<SettingsSerializerAttribute>().Format, serializerType: t);
            }
        }

        public static void Add<TSerializer>(string format) where TSerializer : ISerializer, new()
        {
            ValidateFormat(format);
            _serializerTypes.Add(format, typeof(TSerializer));
        }

        public static void Add(string format, Type serializerType)
        {
            ValidateFormat(format);
            ValidateSerializerType(serializerType);
            _serializerTypes.Add(format, serializerType);
        }

        public static Type GetSerializerType(string format)
        {
            if (!_serializerTypes.TryGetValue(format, out Type serializerType)) throw new ConfigurationException($"Unknown serialization format: '{format}'.");
            return serializerType;
        }

        static void ValidateFormat(string format)
        {
            if (String.IsNullOrWhiteSpace(format)) throw new ConfigurationException($"'{nameof(format)}' argument must not be null or whitespace");
            if (_serializerTypes.ContainsKey(format)) throw new ConfigurationException($"Duplicated serialization format: '{format}'.");
        }

        static void ValidateSerializerType(Type t)
        {
            if (!t.GetInterfaces().Contains(typeof(ISerializer))) throw new ConfigurationException($"Serializer class '{t.Name}' must implement '{typeof(ISerializer).Name}' interface.");
            if (t.GetConstructor(new Type[0]) == null) throw new ConfigurationException($"Serializer class '{t.Name}' must have parameterless ctor.");
        }
    }
}
