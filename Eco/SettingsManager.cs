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
        readonly List<IFieldVisitor> _customLoadVisitors = new List<IFieldVisitor>();
        readonly List<IFieldVisitor> _customSaveVisitors = new List<IFieldVisitor>();
        readonly ISerializer _serializer;
        readonly ISerializationAttributesGenerator _serializationAttributesGenerator;

        public SettingsManager(ISerializer serializer, ISerializationAttributesGenerator serializationAttributesGenerator)
        {
            _serializer = serializer;
            _serializationAttributesGenerator = serializationAttributesGenerator;
            this.DefaultUsage = Usage.Optional;
        }

        public ISerializer Serializer { get { return _serializer; } }

        public ISerializationAttributesGenerator SerializationAttributesGenerator { get { return _serializationAttributesGenerator; } }

        public Usage DefaultUsage { get; set; }

        public List<IFieldVisitor> CustomLoadVisitors { get { return _customLoadVisitors; } }

        public List<IFieldVisitor> CustomSaveVisitors { get { return _customSaveVisitors; } }

        public static string GetDefaultSettingsFileName<T>()
        {
            return GetDefaultSettingsFileName(typeof(T));
        }

        public static string GetDefaultSettingsFileName(Type settingsType)
        {
            return settingsType.Name + ".config";
        }

        public T Load<T>()
        {
            return this.Load<T>(GetDefaultSettingsFileName<T>());
        }

        public T Load<T>(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return this.Read<T>(fileStream);
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

        public T Read<T>(Stream stream, bool skipPostProcessing = false)
        {
            return (T)Read(typeof(T), stream, skipPostProcessing);
        }

        public object Read(Type settingsType, Stream stream, bool skipPostProcessing = false)
        {
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settingsType, this.SerializationAttributesGenerator, this.DefaultUsage);
            object rawSettings = this.Serializer.Deserialize(rawSettingsType, stream);
            return CreateRefinedSettings(settingsType, rawSettings, skipPostProcessing, _customLoadVisitors);
        }

        public void Write(object settings, Stream stream)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settings.GetType(), this.SerializationAttributesGenerator, this.DefaultUsage);
            object rawSettings = CreateRawSettings(rawSettingsType, settings, _customSaveVisitors);
            this.Serializer.Serialize(rawSettings, stream);
        }

        static object CreateRefinedSettings(Type refinedSettingsType, object rawSettings, bool skipPostProcessing, List<IFieldVisitor> customVisitors)
        {
            object refinedSettings = Activator.CreateInstance(refinedSettingsType);
            VisitAllFieldsRecursive(refinedSettings, rawSettings, new RefinedSettingsBuilder());
           if (!skipPostProcessing)
            {
                var settingsMapBuilder = new SettingsMapBuilder();
                VisitAllFieldsRecursive(refinedSettings, rawSettings, settingsMapBuilder);
                VisitAllFieldsRecursive(refinedSettings, rawSettings, new ReferenceResolver(settingsMapBuilder.SettingsById));
                VisitAllFieldsRecursive(refinedSettings, rawSettings, new RequiredFieldChecker());
                VisitAllFieldsRecursive(refinedSettings, rawSettings, new EnvironmentVariableExpander());
                foreach (var customVisitor in customVisitors)
                    VisitAllFieldsRecursive(refinedSettings, rawSettings, customVisitor);
            }
            return refinedSettings;
        }

        static object CreateRawSettings(Type rawSettingsType, object refinedSettings, List<IFieldVisitor> customVisitors)
        {
            object rawSettings = Activator.CreateInstance(rawSettingsType);
            VisitAllFieldsRecursive(refinedSettings, rawSettings, new RawSettingsBuilder());
            VisitAllFieldsRecursive(refinedSettings, rawSettings, new ReferencePacker());
            VisitAllFieldsRecursive(refinedSettings, rawSettings, new RequiredFieldChecker());
            foreach (var customVisitor in customVisitors)
                VisitAllFieldsRecursive(refinedSettings, rawSettings, customVisitor);
            return rawSettings;
        }

        static void VisitAllFieldsRecursive(object refinedSettings, object rawSettings, IFieldVisitor visitor)
        {
            VisitAllFieldsRecursive(refinedSettings.GetType().Name, refinedSettings, rawSettings, visitor, new HashSet<object>());
        }

        static void VisitAllFieldsRecursive(string settingsPath, object refinedSettings, object rawSettings, IFieldVisitor visitor, HashSet<object> visitedSettings)
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

                if (refinedSettingsValue != null && rawSettingsValue != null &&
                    !refinedSettingsField.IsDefined<RefAttribute>() && !rawSettingsField.IsDefined<RefAttribute>())
                {
                    if (refinedSettingsValue.GetType().IsSettingsType())
                    {
                        VisitAllFieldsRecursive(currentPath, refinedSettingsValue, rawSettingsValue, visitor, visitedSettings);
                    }
                    else if (refinedSettingsValue.GetType().IsSettingsArrayType())
                    {
                        var sourceSettingsArray = (Array)refinedSettingsValue;
                        var targetSettingsArray = (Array)rawSettingsValue;
                        for (int i = 0; i < sourceSettingsArray.Length; i++)
                        {
                            currentPath = SettingsPath.Combine(settingsPath, refinedSettingsField.Name, i);
                            VisitAllFieldsRecursive(currentPath, sourceSettingsArray.GetValue(i), targetSettingsArray.GetValue(i), visitor, visitedSettings);
                        }
                    }
                }
            }
        }
    }
}
