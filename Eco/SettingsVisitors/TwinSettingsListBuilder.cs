using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    public class TwinSettingsListBuilder : TwinSettingsVisitorBase
    {
        public List<ITwinSettingsTreeNode> Settings { get; } = new List<ITwinSettingsTreeNode>();

        public override void Initialize(Type rootMasterSettingsType, Type rootSlaveSettingsType) => Settings.Clear();

        public override void Visit(string settingsNamespace, string settingsPath, object masterSettings, object slaveSettings)
        {
            Settings.Add(new TwinSettingsObjectTreeNode(settingsNamespace, settingsPath, masterSettings, slaveSettings));
        }

        public override void Visit(string settingsNamespace, string fieldPath, object masterSettings, FieldInfo masterSettingsField, object slaveSettings, FieldInfo slaveSettingsField)
        {
            Settings.Add(new TwinSettingsFieldTreeNode(
                settingsNamespace,
                fieldPath, 
                masterSettings, 
                masterSettingsField, 
                slaveSettings, 
                slaveSettingsField, 
                masterSettingsField.GetCustomAttribute<SkippedByAttribute>()?.Visitors));
        }
    }
}
