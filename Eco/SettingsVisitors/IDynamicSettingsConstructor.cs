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
    /// Provides a way to notify interested ITwinSettingsVisitor(s) that a new dynamic settings object has been created.
    /// SettingsManager automatically links only default IDynamicSettingsConstuctor(s) and IDynamicSettingsConstructorObserver(s) (i.e. default visitors).
    /// If you override default visiotrs (ie. SettingsManager.RefinedSettingsReadVisitors), you need to care about the linking yourself.
    /// </summary>
    public interface IDynamicSettingsConstructor
    {
        event Action<(string settingsNamesapase, string settingsPath, string settingsId, object rawSettings, object refinedSettings)> SettingsCreated;
    }

    public interface IDynamicSettingsConstructorObserver
    {
        /// <summary>
        /// Eco library calls Observe only once for each default RefinedSettingsReadVisitor.
        /// It doesn't call Observe for any Visitors added manually. 
        /// </summary>
        void Observe(IDynamicSettingsConstructor ctor);
    }
}
