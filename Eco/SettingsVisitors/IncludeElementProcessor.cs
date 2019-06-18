using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Eco.Extensions;
using Eco.Serialization;

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

        protected ISerializer GetSerializer(object includeElem)
        {
            var specifiedFormat = include.GetFormat(includeElem);
            return specifiedFormat != null ?
                (ISerializer)Activator.CreateInstance(SupportedFormats.GetSerializerType(specifiedFormat)) :
                this.Context.Serializer;
        }
    }
}
