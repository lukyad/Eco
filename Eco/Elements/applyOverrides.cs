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

        // Note that ReferenceResolver should skip 'overrides' field, as it could contain invalid references.
        // Reference might become valid only when all defaults/overrides get applied.
        // Given above, ReferenceResolver should be run twice before and after ApplyOverridesProcessor
        [Required, Sealed]
        [SkippedBy(typeof(RequiredFieldChecker), typeof(SettingsMapBuilder), typeof(DefaultValueSetter), typeof(ReferenceResolver))]
        [Doc("Specifies overrides values to be applied to the matched settings.")]
        public T overrides;

        [Optional, Inline]
        [SkippedBy(typeof(DefaultValueSetter))]
        [Doc("Settings filter of the following format: <id-wildcard>[:<type-wildcard>]. Null is equivalent to '*:T'.")]
        public modifyRefList[] refListModifications;

        public static object[] GetTargets(object twin) => (object[])twin.GetFieldValue(nameof(targets));

        public static object GetOverrides(object twin) => twin.GetFieldValue(nameof(overrides));

        public static modifyRefList[] GetRefListModifications(object twin) => (modifyRefList[])twin.GetFieldValue(nameof(refListModifications)) ?? new modifyRefList[0];
    }

    // Shortcut for the static functions.
    public class applyOverrides : applyOverrides<object>
    {
    }
}
