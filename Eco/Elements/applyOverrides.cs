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
    /// Instructs Eco library to override specified fields of all matched settings.
    /// </summary>
    [EcoElement(typeof(applyOverrides<>))]
    public class applyOverrides<T>
    {
        [Optional, Ref]
        [Doc("Settings filter of the following format: <id-wildcard>[:<type-wildcard>]. Null is equivalent to '*:T'.")]
        public T[] targets;

        [Required, Sealed]
        [SkippedBy(typeof(RequiredFieldChecker), typeof(SettingsMapBuilder), typeof(DefaultValueSetter))]
        [Doc("Specifies overrides values to be applied to the matched settings.")]
        public T overrides;

        public static object[] GetTargets(object twin) => (object[])twin.GetFieldValue(nameof(targets));

        public static object GetOverrides(object twin) => twin.GetFieldValue(nameof(overrides));
    }

    // Shortcut for the static functions.
    public class applyOverrides : applyOverrides<object>
    {
    }
}
