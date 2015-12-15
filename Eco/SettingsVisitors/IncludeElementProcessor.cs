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
    public abstract class IncludeElementProcessor  : ISettingsVisitor
    {
        readonly SettingsManager _context;

        public IncludeElementProcessor(SettingsManager context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;
        }

        public SettingsManager Context { get { return _context; } }

        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRawSettingsType) { }

        public void Visit(string settingsNamesapce, string settingsPath, object rawSettings)
        {
            if (rawSettings.IsEcoElementOfGenericType(typeof(include<>)))
                ProcessIncludeElement(rawSettings);
        }

        public void Visit(string settingsNamespace, string fieldPath, FieldInfo settingsField, object settings) { }

        protected abstract void ProcessIncludeElement(object includeElem);
    }
}
