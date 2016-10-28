using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    /// <summary>
    /// Indicates that the given type is not a `Settings` type. 
    /// By default, all types from an assembly marked with the SettingsAssembly attribute are considered to be settings types.
    /// If you don't like a specific type to be processed as a `Settings` type, mark it with NonSettingsTypeAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NonSettingsTypeAttribute : EcoAttribute
    {
    }
}
