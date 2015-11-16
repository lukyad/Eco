using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Common base for IncludeElementReader and IncludeElementWriter.
    /// </summary>
    public abstract class IncludeElementProcessor  : IRawSettingsVisitor
    {
        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootSettingsType) { }

        public void Visit(string fieldPath, FieldInfo rawSettingsField, object rawSettings)
        {
            var fieldType = rawSettingsField.FieldType;
            if (fieldType.IsDefined<IncludeElementAttribute>())
            {
                object includeElem = rawSettingsField.GetValue(rawSettings);
                ProcessIncludeElement((include)includeElem);
            }
            else if (fieldType.IsArray && fieldType.GetElementType().IsDefined<IncludeElementAttribute>())
            {
                object includeElemArray = (Array)rawSettingsField.GetValue(rawSettings);
                foreach (var item in (Array)includeElemArray)
                    ProcessIncludeElement((include)item);
            }
        }

        protected abstract void ProcessIncludeElement(include includeElem);

        public static Type GetIncludedDataType(include includeElem)
        {
            return (Type)includeElem.GetType().GetField(GetDataFieldName()).FieldType;
        }

        public static object GetIncludedData(include includeElem)
        {
            return includeElem.GetFieldValue(GetDataFieldName());
        }

        public static void SetIncludedData(include includeElem, object data)
        {
            includeElem.SetFieldValue(GetDataFieldName(), data);
        }

        public static string GetDataFieldName()
        {
            include<object> includeElemPrototype;
            return nameof(includeElemPrototype.data);
        }
    }
}
