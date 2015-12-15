using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Eco.SettingsVisitors
{
    // Does nothing, but implements stubs for ITwinSettingsVisitor 
    public abstract class TwinSettingsVisitorBase : ITwinSettingsVisitor
    {
        public TwinSettingsVisitorBase(bool isReversable = true)
        {
            this.IsReversable = isReversable;
        }

        public bool IsReversable { get; }


        public virtual void Initialize(Type rootMasterSettingsType, Type rootSlaveSettingsType) { }

        public virtual void Visit(string settingsNamespace, string settingsPath, object masterSettings, object slaveSettings) { }


        public virtual void Visit(string settingsNamespace, string fieldPath, FieldInfo masterSettingsField, object masterSettings, FieldInfo slaveSettingsField, object slaveSettings) { }
    }
}
