using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Common base for ImportElementReader and ImportElementWriter.
    /// </summary>
    public abstract class ImportElementProcessor  : ISettingsVisitor
    {
        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRawSettingsType) { }

        public void Visit(string settingsNamesapce, string settingsPath, object rawSettings)
        {
            if (rawSettings.IsEcoElementOfGenericType(typeof(import<,>)))
                ProcessImportElement(rawSettings);
        }

        public void Visit(string settingsNamespace, string fieldPath, FieldInfo settingsField, object settings) { }

        protected abstract void ProcessImportElement(object includeElem);
    }
}
