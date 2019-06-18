using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;
using Eco.SettingsVisitors;

namespace Eco
{
    /// <summary>
    /// Instructs serializer to include the specified configuration file.
    /// </summary>
    [EcoElement(typeof(include<>)), Doc("Specifies an external file to be included.")]
    public class include<T>
    {
        [Required, Doc("Path (relative or absolute) to the file to be included.")]
        public string file;

        [Optional, Namespace, Name("namespace")]
        [Doc("Namespace to be applied to all object IDs included from the referenced file. If specified, an object from the file should be referenced as '<Namesapce>.<ObjectId>'")]
        public string ns;

        [Optional, Doc("Format of the file to be included (xml, json etc). By default Eco library uses format defined by Settings.DefaultManager.")]
        public string format;

        [Default(true)]
        [Doc("If set to true, Eco library would skip this file when saving the parent configuration file. Default value is true.")]
        public bool? readOnly;

        [Default(false)]
        [Doc("If set to true, Eco library would not trow an exception if the file being included is missing. Default value is false.")]
        public bool? optional;

        [Hidden, SkippedBy(typeof(IncludeElementReader), typeof(IncludeElementWriter))]
        public T data;

        public static string GetFile(object twin) => (string)twin.GetFieldValue(nameof(file));

        public static string GetNamespace(object twin) => (string)twin.GetFieldValue("namespace");

        public static string GetFormat(object twin) => (string)twin.GetFieldValue(nameof(format));

        public static bool IsReadOnly(object twin)
        {
            object isReadOnly = twin.GetFieldValue(nameof(readOnly));
            return isReadOnly.GetType() == typeof(string) ?
                Boolean.Parse(isReadOnly.ToString()) :
                (isReadOnly as bool?).Value;
        }

        public static bool IsOptional(object twin)
        {
            object isOptional = twin.GetFieldValue(nameof(optional));
            return isOptional.GetType() == typeof(string) ?
                Boolean.Parse(isOptional.ToString()) :
                (isOptional as bool?).Value;
        }

        public static object GetData(object twin) => twin.GetFieldValue(nameof(data));

        public static void SetData(object twin, object value) => twin.SetFieldValue(nameof(data), value);

        public static Type GetDataType(object twin) => twin.GetType().GetField(nameof(data)).FieldType;

    }

    // Shortcut for the static functions.
    public class include : include<object>
    {
    }
}
