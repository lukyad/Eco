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
    /// Note that IncludeElementProcessor should support multiVisit as included path could be yet unresolved during the first pass.
    /// </summary>
    public abstract class ImportElementProcessor : SettingsVisitorBase
    {
        public ImportElementProcessor() : base(isReversable: true) { }

        public override void Visit(string settingsNamesapce, string settingsPath, object rawSettings)
        {
            if (rawSettings.IsEcoElementOfGenericType(typeof(import<,>)))
                ProcessImportElement(settingsNamesapce, settingsPath, rawSettings);
        }

        protected abstract void ProcessImportElement(string settingsNamesapce, string settingsPath, object includeElem);
    }
}
