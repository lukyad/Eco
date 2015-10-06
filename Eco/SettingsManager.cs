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
    /// <summary>
    /// Gives full controll under load/save functionality of the Eco library.
    /// </summary>
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
            this.InitializeRefinedSettingsSaveVisitors();
            this.InitializeRawSettingsSaveVisitors();
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

        /// <summary>
        /// Serialized to be used to read/write Eco configuration files.
        /// </summary>
        public ISerializer Serializer { get { return _serializer; } }

        /// <summary>
        /// Attributes generator to bu used by the Eco runtime code emitter.
        /// Shapes generated raw settings types with the attributes required by the Serializer.
        /// </summary>
        public ISerializationAttributesGenerator SerializationAttributesGenerator { get { return _serializationAttributesGenerator; } }

        /// <summary>
        /// Default field usage policy to be used when reading/writing configuration files.
        /// Applies to the settings fields for which usage was not specified explicitly through Optinal or Required attributes.
        /// </summary>
        public Usage DefaultUsage { get; set; }

        /// <summary>
        /// Fields visitors to be invoked right after raw settins object has been deserialized from a source stream.
        /// By default Eco library uses ConfigurationVariableExpander and EnvironmentVariableExpander visitors.
        /// </summary>
        public IEnumerable<IRawSettingsVisitor> RawSettingsLoadVisitors { get; set; }

        /// <summary>
        /// Fields visitors to be invoked right before raw settins object to be serialized to a stream.
        /// Eco library doesn't implement any default save visitors.
        /// </summary>
        public IEnumerable<IRawSettingsVisitor> RawSettingsSaveVisitors { get; set; }

        /// <summary>
        /// Field visitors to be invoked after raw settins load visitors complete thier job.
        /// Default list of the visitors used by the Eco library is defined in the InitializeRefinedSettingsLoadVisitors method.
        /// It's recomended that you append any your custom visitors to the end of the default visitors collection to 
        /// not break refined settins graph construction.
        /// </summary>
        public IEnumerable<IRefinedSettingsVisitor> RefinedSettingsLoadVisitors { get; set; }

        /// <summary>
        /// Field visitors to be invoked as the very first operation on the refined settings graph.
        /// Default list of the visitors used by the Eco library is defined in the InitializeRefinedSettingsSaveVisitors method.
        /// It's recomended that you append any your custom visitors to the end of the default visitors collection to 
        /// not break raw settins graph construction.
        /// </summary>
        public IEnumerable<IRefinedSettingsVisitor> RefinedSettingsSaveVisitors { get; set; }

        /// <summary>
        /// Returns default configuration file name for the specified settins type which is typeof(T).Name + ".config"
        /// </summary>
        public static string GetDefaultSettingsFileName<T>()
        {
            return GetDefaultSettingsFileName(typeof(T));
        }

        /// <summary>
        /// Returns default configuration file name for the specified settins type which is typeof(T).Name + ".config"
        /// </summary>
        public static string GetDefaultSettingsFileName(Type settingsType)
        {
            return settingsType.Name + ".config";
        }

        /// <summary>
        /// Loads settings of the specified type from the default configuration file in the current working directory.
        /// 
        /// Eco library can perform some non-reversable changes under settins when loading them from the configuration file.
        /// Such changes could not be reverted back when saving settings back to the file.
        /// If you are planning to save settings back to the file and want to keep the orginal 'features' of the file, set the skipNonReversableOperations to true.
        /// It would instruct Eco library to skip any field visitors that does non-reversable changes.
        /// 
        /// An example of a visitor that makes non-reversable changes is the EnvironmentVariableExpander visitor.
        /// Once expanded, variables could not be collapsed back.
        /// </summary>
        public T Load<T>(bool skipNonReversableOperations = false)
        {
            return this.Load<T>(GetDefaultSettingsFileName<T>(), skipNonReversableOperations);
        }

        /// <summary>
        /// Loads settings of the specified type from the specified file.
        /// For description of the skipNonReversableOperations argument please check overloaded version of the Load method.
        /// </summary>
        public T Load<T>(string fileName, bool skipNonReversableOperations = false)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return this.Read<T>(fileStream, skipNonReversableOperations);
        }

        /// <summary>
        /// Saves settings to the default configuration file in the current working directory.
        /// </summary>
        public void Save(object settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            this.Save(GetDefaultSettingsFileName(settings.GetType()));
        }

        /// <summary>
        /// Saves settings to the specified file.
        /// </summary>
        public void Save(object settings, string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                this.Write(settings, fileStream);
        }

        /// <summary>
        /// Reads settings of the specified type from a stream.
        /// For description of the skipNonReversableOperations argument please check summary of the Load method.
        /// </summary>
        public T Read<T>(Stream stream, bool skipNonReversableOperations = false)
        {
            return (T)Read(typeof(T), stream, skipNonReversableOperations);
        }

        /// <summary>
        /// Reads settings of the specified type from a stream.
        /// For description of the skipNonReversableOperations argument please check summary of the Load method.
        /// </summary>
        public object Read(Type settingsType, Stream stream, bool skipNonReversableOperations = false)
        {
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settingsType, this.SerializationAttributesGenerator, this.DefaultUsage);
            object rawSettings = this.Serializer.Deserialize(rawSettingsType, stream);
            return CreateRefinedSettings(settingsType, rawSettings, skipNonReversableOperations);
        }

        /// <summary>
        /// Writes settings to the specified stream.
        /// </summary>
        public void Write(object settings, Stream stream)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settings.GetType(), this.SerializationAttributesGenerator, this.DefaultUsage);
            object rawSettings = CreateRawSettings(rawSettingsType, settings);
            this.Serializer.Serialize(rawSettings, stream);
        }

        object CreateRefinedSettings(Type refinedSettingsType, object rawSettings, bool skipNonReversableOperations)
        {
            if (this.RawSettingsLoadVisitors != null)
            {
                foreach (var v in this.RawSettingsLoadVisitors.Where(v => v.IsReversable == !skipNonReversableOperations))
                    VisitRawFieldsRecursive(rawSettings, v);
            }

            object refinedSettings = Activator.CreateInstance(refinedSettingsType);
            if (this.RefinedSettingsLoadVisitors != null)
            {
                foreach (var v in this.RefinedSettingsLoadVisitors.Where(v => v.IsReversable == !skipNonReversableOperations))
                    VisitRefinedFieldsRecursive(refinedSettings, rawSettings, v);
            }

            return refinedSettings;
        }

        object CreateRawSettings(Type rawSettingsType, object refinedSettings)
        {
            object rawSettings = Activator.CreateInstance(rawSettingsType);
            if (this.RefinedSettingsSaveVisitors != null)
            {
                foreach (var v in this.RefinedSettingsSaveVisitors)
                    VisitRefinedFieldsRecursive(refinedSettings, rawSettings, v);
            }
            if (this.RawSettingsSaveVisitors != null)
            {
                foreach (var v in this.RawSettingsSaveVisitors)
                    VisitRawFieldsRecursive(rawSettings, v);
            }
           
            return rawSettings;
        }

        static void VisitRawFieldsRecursive(object rootRawSettings, IRawSettingsVisitor visitor)
        {
            VisitRawFieldsRecursive(rootRawSettings.GetType().Name, rootRawSettings, visitor, visitedSettings: new HashSet<object>());
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

        static void VisitRefinedFieldsRecursive(object rootRefinedSettings, object rawSettings, IRefinedSettingsVisitor visitor)
        {
            visitor.Initialize(rootRefinedSettings.GetType());
            VisitRefinedFieldsRecursive(rootRefinedSettings.GetType().Name, rootRefinedSettings, rawSettings, visitor, new HashSet<object>());
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
