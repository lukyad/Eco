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
    /// Instructs Eco library to initialize all matched settins with default values,
    /// specified be the 'defaults' field.
    /// </summary>
    [EcoElement(typeof(applyDefaults<>))]
    public class applyDefaults<T>
    {
        [Optional, Ref]
        [Doc("Settings filter of the following format: <id-wildcard>[:<type-wildcard>. Null is equivalent to '*:T'.")]
        public T[] targets;

        [Required, Sealed]
        [SkippedBy(typeof(RequiredFieldChecker), typeof(SettingsMapBuilder), typeof(DefaultValueSetter))]
        [Doc("Specifies default values to be applied to the matched settings.")]
        public T defaults;

        public static object[] GetTargets(object twin) => (object[])twin.GetFieldValue(nameof(targets));

        public static object GetDefaults(object twin) => twin.GetFieldValue(nameof(defaults));
    }

    // Shortcut for the static functions.
    public class applyDefaults : applyDefaults<object>
    {
    }
}
