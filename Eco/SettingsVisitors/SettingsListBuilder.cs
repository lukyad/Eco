using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    public class SettingsListBuilder : SettingsVisitorBase
    {
        public List<ISettingsTreeNode> Settings { get; } = new List<ISettingsTreeNode>();

        public override void Initialize(Type rootSettingsType) => Settings.Clear();

        public override void Visit(string settingsNamespace, string settingsPath, object settings)
        {
            Settings.Add(new SettingsObjectTreeNode(settingsNamespace, settingsPath, settings));
        }

        public override void Visit(string settingsNamespace, string fieldPath, object settings, FieldInfo settingsField)
        {
            Settings.Add(new SettingsFieldTreeNode(settingsNamespace, fieldPath, settings, settingsField, settingsField.GetCustomAttribute<SkippedByAttribute>()?.Visitors));
        }
    }
}
