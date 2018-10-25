using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    public class NamespaceMapBuilder : ITwinSettingsVisitor
    {
        readonly Dictionary<object, string> _namespaceMap = new Dictionary<object, string>();

        public bool IsReversable { get { return true; } }

        public Dictionary<object, string> NamespaceMap { get { return _namespaceMap; } }

        public void Initialize(Type rootRefinedSettingsTypme, Type rootRawSettingsType)
        {
            _namespaceMap.Clear();
        }

        public void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings)
        {
            _namespaceMap.Add(refinedSettings, settingsNamespace);
        }

        public void Visit(string settingsNamesapce, string fieldPath, object refinedSettings, FieldInfo refinedSettingsField, object rawSettings, FieldInfo rawSettingsField) { }
    }
}
