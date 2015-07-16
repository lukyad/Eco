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


		public static T Load<T>() where T : new()
		{
			return DefaultManager.Load<T>();
		}

		public static T Load<T>(string fileName) where T : new()
		{
			return DefaultManager.Load<T>(fileName);
		}

		public static void Save<T>(T settings)
		{
			DefaultManager.Save(settings);
		}

		public static void Save<T>(T settings, string fileName)
		{
			DefaultManager.Save(settings, fileName);
		}

		public static T Read<T>(Stream stream) where T : new()
		{
			return DefaultManager.Read<T>(stream);
		}

		public static void Write<T>(T settings, Stream stream)
		{
			DefaultManager.Write<T>(settings, stream);
		}
	}
}
