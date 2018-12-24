using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Any visitor that can override any settings field, must implement this interface.
    /// It provides a way for SettingsManager to track all overriden fields.
    /// If SettingsManager detect a second time override attempt it would raise an exception.
    /// </summary>
    public interface IFieldValueOverrider
    {
        event Action<(object settings, string field)> OverridingField;
    }
}
