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
                ProcessIncludeElement(includeElem);
            }
            else if (fieldType.IsArray && fieldType.GetElementType().IsDefined<IncludeElementAttribute>())
            {
                object includeElemArray = (Array)rawSettingsField.GetValue(rawSettings);
                foreach (var item in (Array)includeElemArray)
                    ProcessIncludeElement(item);
            }
        }

        protected abstract void ProcessIncludeElement(object includeElem);
    }
}
