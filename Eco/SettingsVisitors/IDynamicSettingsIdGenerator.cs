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
    /// Provides a way to notify interested ITwinSettingsVisitor(s) that ID has been assigned to a settings object.
    /// SettingsManager automatically links only default IDynamicSettingsIdGenerator(s) and IDynamicSettingsIdGeneratorObserver(s) (i.e. default visitors).
    /// If you override default visiotrs (ie. SettingsManager.RefinedSettingsReadVisitors), you need to care abount the linking yourself.
    /// </summary>
    public interface IDynamicSettingsIdGenerator
    {
        event Action<(object refinedSettings, string generatedId)> IdGenerated;
    }
}
