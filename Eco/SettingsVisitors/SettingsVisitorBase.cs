using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Eco.SettingsVisitors
{
    // Does nothing, but implements stubs for ISettingsVisitor
    public abstract class SettingsVisitorBase : ISettingsVisitor
    {
        public SettingsVisitorBase(bool isReversable = true)
        {
            IsReversable = isReversable;
        }

        public bool IsReversable { get; }

        public virtual void Initialize(Type rootSettingsType) { }

        public virtual void Visit(string settingsNamespace, string settingsPath, object settings) { }

        public virtual void Visit(string settingsNamespace, string fieldPath, object settings, FieldInfo settingsField) { }
    }
}
