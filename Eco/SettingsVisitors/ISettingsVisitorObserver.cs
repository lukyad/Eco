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
    /// Eco library calls Observe only once for each default RefinedSettingsReadVisitor.
    /// It doesn't call Observe for any Visitors added manually. 
    /// </summary>
    public interface ISettingsVisitorObserver
    {
        void Observe(object visitor);
    }
}
