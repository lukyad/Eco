using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Eco;
using Eco.Serialization.Xml;


namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
			//var t = SerializationTypeEmitter.Emit(typeof(fleet));
			string schema = XmlSchemaExporter.GetSchemaFor<spaceBattle>(Usage.Optional);
			using (var sw = new StreamWriter(@"d:\fleet.xsd"))
				sw.Write(schema);

			using (var inputStream = File.OpenRead("exampleUsage.config"))
			{
				var spaceBattle = SettingsManager.Default.Read<spaceBattle>(inputStream);
				using (var outputStream = File.Create(@"d:\exampleUsage.config"))
					SettingsManager.Default.Write(spaceBattle, outputStream);
			}
		}
    }
}
