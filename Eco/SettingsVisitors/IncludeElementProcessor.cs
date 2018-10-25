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
    /// Common base for IncludeElementReader and IncludeElementWriter.
    /// </summary>
    public abstract class IncludeElementProcessor : SettingsVisitorBase
    {
        public IncludeElementProcessor(SettingsManager context)
            : base(isReversable: true)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public SettingsManager Context { get; }

        public override void Visit(string settingsNamesapce, string settingsPath, object rawSettings)
        {
            if (rawSettings.IsEcoElementOfGenericType(typeof(include<>)))
                ProcessIncludeElement(settingsNamesapce, settingsPath, rawSettings);
        }

        protected abstract void ProcessIncludeElement(string settingsNamesapce, string settingsPath, object includeElem);
    }
}
