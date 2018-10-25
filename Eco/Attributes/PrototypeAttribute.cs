using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.SettingsVisitors;

namespace Eco
{
    /// <summary>
    /// Indicates that the target field defines a prototype object(s).
    /// Prototype objects might have incomplete definition i.e. have some Required fields not set.
    /// Eco library will skip prototype objects when doing Required fileds check.
    /// 
    /// Usage:
    /// Can be applied to a field of a settings or settings array type.
    /// Usually you would create a single wrapper prototy class for your config
    /// It might look like this:
    /// public class prototype
    /// {
    ///     [Inline, Prototype, KnownTypes("*", typeof(yourRootConfigurationType))]
    ///     public object[] definition;
    /// }
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PrototypeAttribute : SkippedByAttribute
    {
        public PrototypeAttribute() :
            base(typeof(RequiredFieldChecker))
        {
        }
    }
}
