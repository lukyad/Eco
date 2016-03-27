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
    /// Instructs Eco library to import settings from the specified file using the specified Serializer.
    /// </summary>
    [EcoElement(typeof(import<,>)), Doc("Instructs serializer to import settings from the specified file using the specified serializer.")]
    public class import<TData, TSerializer>
        where TSerializer : ISerializer, new()
    {
        [Required, Doc("Path (relative or absolute) to the file to be imported.")]
        public string file;

        [Default(true)]
        [Doc("If set to true, then Eco library would skip this file when saving the parent configuration file. Default value is true.")]
        public bool? readOnly;

        [Hidden]
        public TData data;

        // This is an auxilary field that won't be serialized and won't appear in the configuration schema.
        // We need it to pinpoint type of the Serializer when processing the `import<>` element.
        [Hidden]
        public TSerializer serializer;

        public static string GetFile(object twin) => (string)twin.GetFieldValue(nameof(file));

        public static Type GetSerializerType(object twin) => twin.GetType().GetField(nameof(serializer)).FieldType;

        public static object GetData(object twin) => twin.GetFieldValue(nameof(data));

        public static void SetData(object twin, object value) => twin.SetFieldValue(nameof(data), value);

        public static Type GetDataType(object twin) => twin.GetType().GetField(nameof(data)).FieldType;

        public static bool IsReadOnly(object twin)
        {
            object isReadOnly = twin.GetFieldValue(nameof(readOnly));
            return isReadOnly.GetType() == typeof(string) ?
                isReadOnly.ToString() == Boolean.TrueString :
                (isReadOnly as bool?).Value;
        }
    }

    // Shortcut for the static functions.
    public class import : import<object, import.MockSerializer>
    {
        public  class MockSerializer : ISerializer
        {
            public object Deserialize(Type rawSettingsType, TextReader stream)
            {
                throw new NotImplementedException();
            }

            public void Serialize(object rawSettings, TextWriter stream)
            {
                throw new NotImplementedException();
            }
        }
    }
}
