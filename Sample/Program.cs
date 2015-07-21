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
            string schema = XmlSchemaExporter.GetSchemaFor<spaceBattle>(defaultUsage: Usage.Optional);
            using (var sw = new StreamWriter(@"d:\spaceBattle.xsd"))
                sw.Write(schema);

            string settingsFileName = "exampleUsage.config";
            spaceBattle settings = Settings.Load<spaceBattle>(settingsFileName);
            Settings.Save(settings, settingsFileName);
        }
    }
}
