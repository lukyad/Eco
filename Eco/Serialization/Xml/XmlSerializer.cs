using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using Eco.Extensions;
using SystemXmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace Eco.Serialization.Xml
{
    public class XmlSerializer : ISerializer
    {
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public object Deserialize(Type rawSettingsType, TextReader reader)
        {
            var serializer = new SystemXmlSerializer(rawSettingsType);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            serializer.UnknownElement += new XmlElementEventHandler(serializer_UnknownElement);
            return serializer.Deserialize(reader);
        }

        public void Serialize(object rawSettings, TextWriter writer)
        {
            var settingsType = rawSettings.GetType();
            var serializer = new SystemXmlSerializer(settingsType);

            // this is required to omit unused xml namespace declarations
            var ns = new XmlSerializerNamespaces();
            ns.Add("", XmlAttributesGenerator.GetXmlNamesapceForRootType(settingsType));

            // this is required to omit <?xml... 
            var xws = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Auto,
                Indent = true,
                Encoding = this.Encoding
            };

            using (var xw = XmlWriter.Create(writer, xws))
                serializer.Serialize(xw, rawSettings, ns);
        }

        public void GenerateSerializationAssembly(Type[] rawSettingsTypes)
        {
            if (rawSettingsTypes == null) throw new ArgumentNullException(nameof(rawSettingsTypes));
            if (rawSettingsTypes.Length == 0) throw new ArgumentException($"{nameof(rawSettingsTypes)} should not be empty.");
            var rawTypesAssembly = rawSettingsTypes.Select(t => t.Assembly).Distinct().SingleOrDefault();
            if (rawTypesAssembly == null) throw new ArgumentException($"All {nameof(rawSettingsTypes)} should belong to the same assembly.");

            var xmlReflectionImporter = new XmlReflectionImporter();
            var mappings = rawSettingsTypes
                .Select(t => xmlReflectionImporter.ImportTypeMapping(t))
                .ToArray();

            string destFilePath = Path.Combine(Path.GetDirectoryName(rawTypesAssembly.Location), SystemXmlSerializer.GetXmlSerializerAssemblyName(rawSettingsTypes.First()) + ".dll");
            SystemXmlSerializer.GenerateSerializer(rawSettingsTypes, mappings, new CompilerParameters { OutputAssembly = destFilePath });
        }

        static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            throw new ConfigurationException(
                "Unknown xml attribute :'{0}' line '{1}', position '{2}'. Expected elements - '{3}'.",
                e.Attr.Name,
                e.LineNumber,
                e.LinePosition,
                e.ExpectedAttributes
            );
        }

        static void serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            throw new ConfigurationException(
                "Unknown xml element :'{0}' line '{1}', position '{2}'. Expected elements - '{3}'.",
                e.Element.Name,
                e.LineNumber,
                e.LinePosition,
                e.ExpectedElements
            );
        }
    }
}
