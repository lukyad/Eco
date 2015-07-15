using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;
using Eco.Extensions;
using SystemXmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace Eco.Serialization.Xml
{
	public class XmlSerializer : ISerializaer
	{
		public object Deserialize(Type rawSettingsType, Stream stream)
		{
			var serializer = new SystemXmlSerializer(rawSettingsType);
			serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
			serializer.UnknownElement += new XmlElementEventHandler(serializer_UnknownElement);

			object xmlSettings;
			using (var sr = new StreamReader(stream))
				xmlSettings = serializer.Deserialize(sr);

			return xmlSettings;
		}

		public void Serialize(object rawSettings, Stream stream)
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
				Indent = true
			};

			using (var xw = XmlWriter.Create(stream, xws))
				serializer.Serialize(xw, rawSettings, ns);
		}

		static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			throw new SerializationException(String.Format(
				"Unknown xml attribute :'{0}' line '{1}', position '{2}'. Expected elements - '{3}'.",
				e.Attr.Name,
				e.LineNumber,
				e.LinePosition,
				e.ExpectedAttributes
			));
		}

		static void serializer_UnknownElement(object sender, XmlElementEventArgs e)
		{
			throw new SerializationException(String.Format(
				"Unknown xml element :'{0}' line '{1}', position '{2}'. Expected elements - '{3}'.",
				e.Element.Name,
				e.LineNumber,
				e.LinePosition,
				e.ExpectedElements
			));
		}
	}
}
