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
        readonly List<ISettingsTreeNode> _settings = new List<ISettingsTreeNode>();

        public IReadOnlyList<ISettingsTreeNode> Settings => _settings;

        public override void Initialize(Type rootSettingsType) => _settings.Clear();

        public override void Visit(string settingsNamespace, string settingsPath, object settings)
        {
            _settings.Add(new SettingsObjectTreeNode(settingsNamespace, settingsPath, settings));
        }

        public override void Visit(string settingsNamespace, string fieldPath, object settings, FieldInfo settingsField)
        {
            _settings.Add(new SettingsFieldTreeNode(settingsNamespace, fieldPath, settings, settingsField, settingsField.GetCustomAttribute<SkippedByAttribute>()?.Visitors));
        }
    }
}
