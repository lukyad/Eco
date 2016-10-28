using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.Serialization.Csv
{
    /// <summary>
    /// Used to serialize an array of settings to the csv formatted file.
    /// Current limitations:
    /// 1. All columns represented in the file, must correspond to a field of the settings being deserialized.
    /// 2. Serializer supports only fields of the String type.
    /// </summary>
    public class CsvSerializer : ISerializer
    {
        /// <summary>
        /// Columns to be serialized. Each column represent a field of the serialized object. Used only when writing settings to the file.
        /// If null, then serializer willl create one column for each public field of the serialized object.
        /// </summary>
        public string[] Columns { get; set; } = null;

        /// <summary>
        /// Delimiter of the values.
        /// </summary>
        public string Delimiter { get; set; } = ",";

        public void Serialize(object rawSettingsArray, TextWriter writer)
        {
            if (rawSettingsArray == null) throw new ArgumentNullException(nameof(rawSettingsArray));
            if (!rawSettingsArray.GetType().IsArray) throw new ArgumentException($"Invalid argument `{nameof(rawSettingsArray)}`. Expected an object of an Array type.");
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            Type elemType = rawSettingsArray.GetType().GetElementType();
            if (this.Columns == null) this.Columns = elemType.GetFields().Select(f => f.Name).ToArray();
            ValidateColumns(this.Columns, elemType);
            // Write header
            writer.WriteLine(String.Join(this.Delimiter, this.Columns));
            // Write data
            Array data = (Array)rawSettingsArray;
            for (int i = 0; i < data.Length; i++)
                writer.WriteLine(ToCsv(data.GetValue(i)));
        }

        public object Deserialize(Type rawSettingsArrayType, TextReader reader)
        {
            if (rawSettingsArrayType == null) throw new ArgumentNullException(nameof(rawSettingsArrayType));
            if (!rawSettingsArrayType.IsArray) throw new ArgumentException($"Invalid argument `{nameof(rawSettingsArrayType)}`. Expected an Array type.");
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            // Validate header.
            Type elemType = rawSettingsArrayType.GetElementType();
            string[] columns = reader.ReadLine().Split(new[] { this.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
            ValidateColumns(columns, elemType);
            // Read data.
            string[] lines = reader.ReadToEnd().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            Array data = Array.CreateInstance(elemType, lines.Length);
            for (int i = 0; i < data.Length; i++)
                data.SetValue(FromCsv(lines[i], elemType, columns), i);

            return data;
        }

        static void ValidateColumns(string[] columns, Type serializedObjectType)
        {
            var fieldNames = new HashSet<string>(serializedObjectType.GetFields().Select(f => f.Name));
            foreach (var c in columns)
                if (!fieldNames.Contains(c))
                    throw new ConfigurationException($"Invalid csv column name: `{c}`.");
        }

        string ToCsv(object obj)
        {
            var values = this.Columns.Select(c => obj.GetFieldValue(c).ToString());
            return String.Join(this.Delimiter, values);
        }

        object FromCsv(string csv, Type objType, string[] columns)
        {
            var obj = Activator.CreateInstance(objType);
            var values = csv.Split(new[] { this.Delimiter }, StringSplitOptions.None);
            for (int i = 0; i < columns.Length; i++)
            {
                if (!String.IsNullOrEmpty(values[i]))
                    obj.SetFieldValue(columns[i], values[i]);
            }
            return obj;
        }
    }
}
