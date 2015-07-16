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
			string schema = XmlSchemaExporter.GetSchemaFor<spaceBattle>(Usage.Optional);
			using (var sw = new StreamWriter(@"d:\fleet.xsd"))
				sw.Write(schema);

			using (var inputStream = File.OpenRead("exampleUsage.config"))
			{
				var settingsFileName = @"d:\exampleUsage.config";
                var spaceBattle = Settings.Load<spaceBattle>(settingsFileName);
				Settings.Save(spaceBattle, settingsFileName);
			}
		}
    }
}
