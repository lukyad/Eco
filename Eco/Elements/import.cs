using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;
using Eco.Serialization;
using Eco.SettingsVisitors;

namespace Eco
{
    /// <summary>
    /// Instructs Eco library to import settings using the specified Importer.
    /// If Importer requires any parameters, then import<,> class needs to be inherited
    /// and all parameters need to be specified in the inherited class, 
    /// the instance of that class will be provided to the Importer's c'tor.
    /// Thus, Importer class must have a constructor accepting instance of the import<> element as a single parameter.
    /// </summary>
    [EcoElement(typeof(import<,>)), Doc("Instructs serializer to import settings using the specified importer.")]
    public class import<TData, TImporter>
        where TImporter : IImporter
    {
        public import()
        {
            // Validate Importer's c'tor
            var validCtor = typeof(TImporter).GetConstructor(new[] { GetType() });
            if (validCtor == null)
            {
                throw new ConfigurationException(
                    "Invalid import<,> element: dataType='{0}', importerType='{1}'. Importer class must have a constructor accepting an instance of the import element, specified earlier, as a single parameter.",
                    typeof(TData).Name, typeof(TImporter).Name);
            }
        }

        [Default(false)]
        [Doc("If set to true, Eco library would not trow an exception if the file being included is missing. Default value is false.")]
        public bool? optional;

        [Hidden]
        public TData data;

        // This is an auxilary field that won't appear in the configuration schema.
        // We need it to pinpoint Importer type when processing the `import<>` element.
        [Hidden]
        public TImporter importer;

        public static Type GetImporterType(object twin) => twin.GetType().GetField(nameof(importer)).FieldType;

        public static object GetData(object twin) => twin.GetFieldValue(nameof(data));

        public static void SetData(object twin, object value) => twin.SetFieldValue(nameof(data), value);

        public static Type GetDataType(object twin) => twin.GetType().GetField(nameof(data)).FieldType;

        public static bool IsOptional(object twin)
        {
            object isOptional = twin.GetFieldValue(nameof(optional));
            return isOptional.GetType() == typeof(string) ?
                Boolean.Parse(isOptional.ToString()) :
                (isOptional as bool?).Value;
        }
    }

    // Shortcut for the static functions.
    public class import : import<object, import.MockImporter>
    {
        public class MockImporter : IImporter
        {
            public object Import() => throw new NotImplementedException();
        }
    }
}
