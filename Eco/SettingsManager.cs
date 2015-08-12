using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Eco.Extensions;
using Eco.Serialization;
using Eco.Serialization.Xml;
using Eco.FieldVisitors;

namespace Eco
{
    public class SettingsManager
    {
        readonly ISerializer _serializer;
        readonly ISerializationAttributesGenerator _serializationAttributesGenerator;

        public SettingsManager(ISerializer serializer, ISerializationAttributesGenerator serializationAttributesGenerator)
        {
            _serializer = serializer;
            _serializationAttributesGenerator = serializationAttributesGenerator;
            this.DefaultUsage = Usage.Optional;
            this.InitializeRawSettingsLoadVisitors();
            this.InitializeRefinedSettingsLoadVisitors();
        }

        void InitializeRawSettingsLoadVisitors()
        {
            var variableMapBuilder = new ConfigurationVariableMapBuilder();
            this.RawSettingsLoadVisitors = new List<IRawSettingsVisitor>
            {
                variableMapBuilder,
                new ConfigurationVariableExpander(variableMapBuilder.Variables),
                new EnvironmentVariableExpander()
            };
        }

        void InitializeRefinedSettingsLoadVisitors()
        {
            var settingsMapBuilder = new SettingsMapBuilder();
            this.RefinedSettingsLoadVisitors = new List<IRefinedSettingsVisitor>
            {
                new RefinedSettingsBuilder(),
                settingsMapBuilder,
                new ReferenceResolver(settingsMapBuilder.SettingsById),
                new RequiredFieldChecker()
            };
        }

        void InitializeRawSettingsSaveVisitors()
        {
        }

        void InitializeRefinedSettingsSaveVisitors()
        {
            this.RefinedSettingsSaveVisitors = new List<IRefinedSettingsVisitor>
            {
                new RequiredFieldChecker(),
                new RawSettingsBuilder(),
                new ReferencePacker(),
            };
        }

        public ISerializer Serializer { get { return _serializer; } }

        public ISerializationAttributesGenerator SerializationAttributesGenerator { get { return _serializationAttributesGenerator; } }

        public Usage DefaultUsage { get; set; }

        public IEnumerable<IRawSettingsVisitor> RawSettingsLoadVisitors { get; set; }

        public IEnumerable<IRawSettingsVisitor> RawSettingsSaveVisitors { get; set; }

        public IEnumerable<IRefinedSettingsVisitor> RefinedSettingsLoadVisitors { get; set; }

        public IEnumerable<IRefinedSettingsVisitor> RefinedSettingsSaveVisitors { get; set; }

        public static string GetDefaultSettingsFileName<T>()
        {
            return GetDefaultSettingsFileName(typeof(T));
        }

        public static string GetDefaultSettingsFileName(Type settingsType)
        {
            return settingsType.Name + ".config";
        }

        public T Load<T>(bool skipNonReversableOperations = false)
        {
            return this.Load<T>(GetDefaultSettingsFileName<T>(), skipNonReversableOperations);
        }

        public T Load<T>(string fileName, bool skipNonReversableOperations = false)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return this.Read<T>(fileStream, skipNonReversableOperations);
        }

        public void Save(object settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            this.Save(GetDefaultSettingsFileName(settings.GetType()));
        }

        public void Save(object settings, string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                this.Write(settings, fileStream);
        }

        public T Read<T>(Stream stream, bool skipNonReversableOperations = false)
        {
            return (T)Read(typeof(T), stream, skipNonReversableOperations);
        }

        public object Read(Type settingsType, Stream stream, bool skipNonReversableOperations = false)
        {
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settingsType, this.SerializationAttributesGenerator, this.DefaultUsage);
            object rawSettings = this.Serializer.Deserialize(rawSettingsType, stream);
            return CreateRefinedSettings(settingsType, rawSettings, skipNonReversableOperations);
        }

        public void Write(object settings, Stream stream)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settings.GetType(), this.SerializationAttributesGenerator, this.DefaultUsage);
            object rawSettings = CreateRawSettings(rawSettingsType, settings);
            this.Serializer.Serialize(rawSettings, stream);
        }

        object CreateRefinedSettings(Type refinedSettingsType, object rawSettings, bool skipNonReversableOperations)
        {
            foreach (var v in this.RawSettingsLoadVisitors.Where(v => v.IsReversable == !skipNonReversableOperations))
                VisitRawFieldsRecursive(rawSettings, v);

            object refinedSettings = Activator.CreateInstance(refinedSettingsType);
            foreach (var v in this.RefinedSettingsLoadVisitors.Where(v => v.IsReversable == !skipNonReversableOperations))
                VisitRefinedFieldsRecursive(refinedSettings, rawSettings, v);

            return refinedSettings;
        }

        object CreateRawSettings(Type rawSettingsType, object refinedSettings)
        {
            object rawSettings = Activator.CreateInstance(rawSettingsType);
            foreach (var v in this.RefinedSettingsSaveVisitors)
                VisitRefinedFieldsRecursive(refinedSettings, rawSettings, v);

            foreach (var v in this.RawSettingsSaveVisitors)
                VisitRawFieldsRecursive(rawSettings, v);
           
            return rawSettings;
        }

        static void VisitRawFieldsRecursive(object rawSettings, IRawSettingsVisitor visitor)
        {
            VisitRawFieldsRecursive(rawSettings.GetType().Name, rawSettings, visitor, new HashSet<object>());
        }

        static void VisitRawFieldsRecursive(string settingsPath, object rawSettings, IRawSettingsVisitor visitor, HashSet<object> visitedSettings)
        {
            if (visitedSettings.Contains(rawSettings)) return;
            visitedSettings.Add(rawSettings);

            foreach (var rawSettingsField in rawSettings.GetType().GetFields())
            {
                string currentPath = SettingsPath.Combine(settingsPath, rawSettingsField.Name);
                visitor.Visit(currentPath, rawSettingsField, rawSettings);

                object rawSettingsValue = rawSettingsField.GetValue(rawSettings);
                if (rawSettingsValue != null)
                {
                    if (rawSettingsValue.GetType().IsSettingsType())
                    {
                        VisitRawFieldsRecursive(currentPath, rawSettingsValue, visitor, visitedSettings);
                    }
                    else if (rawSettingsValue.GetType().IsSettingsArrayType())
                    {
                        var arr = (Array)rawSettingsValue;
                        for (int i = 0; i < arr.Length; i++)
                        {
                            currentPath = SettingsPath.Combine(settingsPath, rawSettingsField.Name, i);
                            VisitRawFieldsRecursive(currentPath, arr.GetValue(i), visitor, visitedSettings);
                        }
                    }
                }
            }
        }

        static void VisitRefinedFieldsRecursive(object refinedSettings, object rawSettings, IRefinedSettingsVisitor visitor)
        {
            VisitRefinedFieldsRecursive(refinedSettings.GetType().Name, refinedSettings, rawSettings, visitor, new HashSet<object>());
        }

        static void VisitRefinedFieldsRecursive(string settingsPath, object refinedSettings, object rawSettings, IRefinedSettingsVisitor visitor, HashSet<object> visitedSettings)
        {
            if (visitedSettings.Contains(refinedSettings)) return;
            visitedSettings.Add(refinedSettings);

            foreach (var refinedSettingsField in refinedSettings.GetType().GetFields())
            {
                var rawSettingsField = rawSettings.GetType().GetField(refinedSettingsField.Name);
                string currentPath = SettingsPath.Combine(settingsPath, refinedSettingsField.Name);
                visitor.Visit(currentPath, refinedSettingsField, refinedSettings, rawSettingsField, rawSettings);

                object refinedSettingsValue = refinedSettingsField.GetValue(refinedSettings);
                object rawSettingsValue = refinedSettingsField.IsDefined<FieldMutatorAttribute>() ?
                    refinedSettingsField.GetCustomAttribute<FieldMutatorAttribute>().GetRawSettingsFieldValue(rawSettingsField, rawSettings) :
                    rawSettingsField.GetValue(rawSettings);

                if (!refinedSettingsField.IsDefined<RefAttribute>() && refinedSettingsValue != null && rawSettingsValue != null)
                {
                    if (refinedSettingsValue.GetType().IsSettingsType())
                    {
                        VisitRefinedFieldsRecursive(currentPath, refinedSettingsValue, rawSettingsValue, visitor, visitedSettings);
                    }
                    else if (refinedSettingsValue.GetType().IsSettingsArrayType())
                    {
                        var refinedSettingsArray = (Array)refinedSettingsValue;
                        var rawSettingsArray = (Array)rawSettingsValue;
                        for (int i = 0; i < refinedSettingsArray.Length; i++)
                        {
                            currentPath = SettingsPath.Combine(settingsPath, refinedSettingsField.Name, i);
                            VisitRefinedFieldsRecursive(currentPath, refinedSettingsArray.GetValue(i), rawSettingsArray.GetValue(i), visitor, visitedSettings);
                        }
                    }
                }
            }
        }
    }
}
