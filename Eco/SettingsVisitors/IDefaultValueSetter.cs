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
    /// Any visitor that can initialize any settings field, must implement this interface.
    /// It provides a way for SettingsManager to track all fields initialized with a default value by a SettingsVisitor.
    /// If SettingsManager detect a second time initialization attempt it would raise an exception.
    /// </summary>
    public interface IDefaultValueSetter
    {
        // <settings, filedPath>
        event Action<(object settings, string field)> InitializingField;
    }
}
