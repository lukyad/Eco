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
    /// Used by applyOverrides Eco element.
    /// Allows to modify list of setting references (e.g. any field of an array type marked with the Ref attribute).
    /// </summary>
    [EcoElement(typeof(modifyRefList)), Doc("Allows to modify list of setting references.")]
    public class modifyRefList
    {
        [Doc("Name of the filed that contains list of setting references to be modified.")]
        public string field;

        [Inline, Rename("refList(.+)", "$1")]
        public refListCommand[] commands;
    }

    public abstract class refListCommand
    {
        // Please note that we define items as an reference array (not a string)
        // This is required to catch any missing references before the list modification gets applied.
        [Required, Ref, Doc("Settings reference(s).")]
        public object[] items;

        public IEnumerable<object> NotNullItems => items.Where(i => i != null);

        public abstract object[] Apply(object[] sourceList);
    }

    [EcoElement(typeof(refListAddBack)), Doc("Adds new reference(s) to the end of the list.")]
    public class refListAddBack : refListCommand
    {
        public override object[] Apply(object[] sourceList) => sourceList.Concat(NotNullItems).ToArray();
    }

    [EcoElement(typeof(refListAddBack)), Doc("Adds new reference(s) to the begining of the list.")]
    public class refListAddFront : refListCommand
    {
        public override object[] Apply(object[] sourceList) => NotNullItems.Concat(sourceList).ToArray();
    }

    [EcoElement(typeof(refListAddBack)), Doc("Inserts new reference(s) after (or before) the specified item.")]
    public class refListInsert : refListCommand
    {
        [Ref, Doc("Reference after that all items to be inserted.")]
        public object after;

        [Ref, Doc("Reference before that all items to be inserted.")]
        public object before;

        public override object[] Apply(object[] sourceList)
        {
            if (after == null && before == null)
                throw new ConfigurationException("refListInsert: both 'after' and 'before' either are not specified or resolved to null.");

            if (after != null && before != null)
                throw new ConfigurationException("refListInsert: please specify either 'after' of 'before', but not both.");

            int targetIndex = Array.FindIndex(sourceList, i => ReferenceEquals(i, after ?? before));
            if (targetIndex == -1)
                throw new ConfigurationException("refListInsert: could not find the place to insert items at, after={0}, before={1}", after, before);

            int insertAt = after != null ? targetIndex + 1 : targetIndex;
            return sourceList.Take(insertAt).Concat(NotNullItems).Concat(sourceList.Skip(insertAt)).ToArray();
        }
    }

    [EcoElement(typeof(refListAddBack)), Doc("Removes specified items from the reference list.")]
    public class refListRemove : refListCommand
    {
        public override object[] Apply(object[] sourceList) => sourceList.Where(i => !NotNullItems.Any(toRemove => ReferenceEquals(toRemove, i))).ToArray();
    }
}
