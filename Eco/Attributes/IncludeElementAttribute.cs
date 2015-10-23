using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;

namespace Eco
{
    /// <summary>
    /// Used internally by the Eco library.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IncludeElementAttribute : EcoAttribute
    {
    }
}
