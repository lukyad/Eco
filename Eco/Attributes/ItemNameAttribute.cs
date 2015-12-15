using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Provides an extra constructor to the Rename attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ItemNameAttribute : RenameAttribute
    {
        public ItemNameAttribute(string elementName)
            : base(".+", elementName)
        {
        }
    }
}
