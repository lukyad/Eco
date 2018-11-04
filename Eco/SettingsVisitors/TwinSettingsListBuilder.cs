using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    public class TwinSettingsListBuilder : TwinSettingsVisitorBase, IDynamicSettingsConstructorObserver
    {
        readonly List<ITwinSettingsTreeNode> _settings = new List<ITwinSettingsTreeNode>();

        public IReadOnlyList<ITwinSettingsTreeNode> Settings => _settings;

        public override void Initialize(Type rootMasterSettingsType, Type rootSlaveSettingsType) => _settings.Clear();

        public override void Visit(string settingsNamespace, string settingsPath, object masterSettings, object slaveSettings)
        {
            _settings.Add(new TwinSettingsObjectTreeNode(settingsNamespace, settingsPath, masterSettings, slaveSettings));
        }

        public override void Visit(string settingsNamespace, string fieldPath, object masterSettings, FieldInfo masterSettingsField, object slaveSettings, FieldInfo slaveSettingsField)
        {
            _settings.Add(new TwinSettingsFieldTreeNode(
                settingsNamespace,
                fieldPath,
                masterSettings,
                masterSettingsField,
                slaveSettings,
                slaveSettingsField,
                masterSettingsField.GetCustomAttribute<SkippedByAttribute>()?.Visitors));
        }

        public void Observe(IDynamicSettingsConstructor ctor)
        {
            ctor.SettingsCreated += s =>
            {
                SettingsManager.TraverseTwinSeetingsTrees(
                    startNamespace: s.settingsNamesapase,
                    startPath: s.settingsPath,
                    rootMasterSettings: s.refinedSettings,
                    rootSlaveSettings: s.rawSettings,
                    visitor: this,
                    initVisitor: false);
            };
        }
    }
}
