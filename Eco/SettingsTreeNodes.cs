using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Supports internal Eco infrastructure. Is not supposed to be used externally.
    /// </summary>
    public interface ISettingsTreeNode
    {
        string Path { get; }
        bool SkippedBy(ISettingsVisitor visitor);
        void Accept(ISettingsVisitor visitor);
    }

    public interface ITwinSettingsTreeNode
    {
        string Path { get; }
        bool SkippedBy(ITwinSettingsVisitor visitor);
        void Accept(ITwinSettingsVisitor visitor);
    }

    class SettingsObjectTreeNode : ISettingsTreeNode
    {
        readonly string _namespace;
        readonly object _settings;

        public SettingsObjectTreeNode(string settingsNamespace, string settingsPath, object settings)
        {
            _namespace = settingsNamespace;
            Path = settingsPath;
            _settings = settings;
        }

        public string Path { get; }

        public bool SkippedBy(ISettingsVisitor visitor) => false;

        public void Accept(ISettingsVisitor visitor) => visitor.Visit(_namespace, Path, _settings);
    }

    class SettingsFieldTreeNode : ISettingsTreeNode
    {
        readonly string _namespace;
        readonly object _settings;
        readonly FieldInfo _field;
        readonly HashSet<Type> _skippedByVisitors;

        public SettingsFieldTreeNode(string settingsNamespace, string fieldPath, object settings, FieldInfo settingsField, HashSet<Type> skippedByVisitors)
        {
            _namespace = settingsNamespace;
            Path = fieldPath;
            _settings = settings;
            _field = settingsField;
            _skippedByVisitors = skippedByVisitors;
        }

        public string Path { get; }

        public bool SkippedBy(ISettingsVisitor visitor) => _skippedByVisitors?.Contains(visitor.GetType()) ?? false;

        public void Accept(ISettingsVisitor visitor) => visitor.Visit(_namespace, Path, _settings, _field);
    }

    class TwinSettingsObjectTreeNode : ITwinSettingsTreeNode
    {
        readonly string _namespace;
        readonly object _masterSettings;
        readonly object _slaveSettings;

        public TwinSettingsObjectTreeNode(string settingsNamespace, string settingsPath, object masterSettings, object slaveSettings)
        {
            _namespace = settingsNamespace;
            Path = settingsPath;
            _masterSettings = masterSettings;
            _slaveSettings = slaveSettings;
        }

        public string Path { get; }

        public bool SkippedBy(ITwinSettingsVisitor visitor) => false;

        public void Accept(ITwinSettingsVisitor visitor) => visitor.Visit(_namespace, Path, _masterSettings, _slaveSettings);
    }

    class TwinSettingsFieldTreeNode : ITwinSettingsTreeNode
    {
        readonly string _namespace;
        readonly object _masterSettings;
        readonly FieldInfo _masterSettingsField;
        readonly object _slaveSettings;
        readonly FieldInfo _slaveSettingsField;
        readonly HashSet<Type> _skippedByVisitors;

        public TwinSettingsFieldTreeNode(
            string settingsNamespace, 
            string settingsPath, 
            object masterSettings, 
            FieldInfo masterSettingsField, 
            object slaveSettings,
            FieldInfo slaveSettingsField,
            HashSet<Type> skippedByVisitors)
        {
            _namespace = settingsNamespace;
            Path = settingsPath;
            _masterSettings = masterSettings;
            _masterSettingsField = masterSettingsField;
            _slaveSettings = slaveSettings;
            _slaveSettingsField = slaveSettingsField;
            _skippedByVisitors = skippedByVisitors;
        }

        public string Path { get; }

        public bool SkippedBy(ITwinSettingsVisitor visitor) => _skippedByVisitors?.Contains(visitor.GetType()) ?? false;

        public void Accept(ITwinSettingsVisitor visitor) => visitor.Visit(_namespace, Path, _masterSettings, _masterSettingsField, _slaveSettings, _slaveSettingsField);
    }
}
