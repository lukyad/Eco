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
using Eco.SettingsVisitors;

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
            this.DefaultUsage = Usage.Required;
            this.InitializeRawSettingsLoadVisitors();
            this.InitializeRefinedSettingsLoadVisitors();
            this.InitializeRefinedSettingsSaveVisitors();
            this.InitializeRawSettingsSaveVisitors();
        }

        void InitializeRawSettingsLoadVisitors()
        {
            var variableMapBuilder = new ConfigurationVariableMapBuilder();
            this.RawSettingsReadVisitors = new List<IRawSettingsVisitor>
            {
                variableMapBuilder,
                new ConfigurationVariableExpander(variableMapBuilder.Variables),
                new EnvironmentVariableExpander(),
                new IncludeElementReader(this)
            };
        }

        void InitializeRefinedSettingsLoadVisitors()
        {
            var settingsMapBuilder = new SettingsMapBuilder();
            this.RefinedSettingsReadVisitors = new List<IRefinedSettingsVisitor>
            {
                new RefinedSettingsBuilder(),
                settingsMapBuilder,
                new ReferenceResolver(settingsMapBuilder.SettingsById),
                new RequiredFieldChecker()
            };
        }

        void InitializeRawSettingsSaveVisitors()
        {
            this.RawSettingsWriteVisitors = new List<IRawSettingsVisitor>
            {
                new IncludeElementWriter(this)
            };
        }

        void InitializeRefinedSettingsSaveVisitors()
        {
            this.RefinedSettingsWriteVisitors = new List<IRefinedSettingsVisitor>
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

        /// Eco library can perform some non-reversable changes under settins when loading them from the configuration file.
        /// Such changes could not be reverted back when saving settings back to the file.
        /// If you are planning to save settings back to the file and want to keep the orginal 'features' of the file, set the SkipNonReversableOperations to true.
        /// It would instruct Eco library to skip any field visitors that does non-reversable changes.
        /// 
        /// An example of a visitor that makes non-reversable changes is the EnvironmentVariableExpander visitor.
        /// Once expanded, variables could not be collapsed back.
        /// 
        /// By default SkipNonReversableOperations is set to false.
        public bool SkipNonReversableOperations { get; set; }

        /// <summary>
        /// Fields visitors to be invoked right after raw settins object has been deserialized from a source stream.
        /// By default Eco library uses ConfigurationVariableExpander and EnvironmentVariableExpander visitors.
        /// </summary>
        public List<IRawSettingsVisitor> RawSettingsReadVisitors { get; set; }

        /// <summary>
        /// Fields visitors to be invoked right before raw settins object to be serialized to a stream.
        /// Eco library doesn't implement any default save visitors.
        /// </summary>
        public List<IRawSettingsVisitor> RawSettingsWriteVisitors { get; set; }

        /// <summary>
        /// Field visitors to be invoked after raw settins load visitors complete thier job.
        /// Default list of the visitors used by the Eco library is defined in the InitializeRefinedSettingsLoadVisitors method.
        /// It's recomended that you append any your custom visitors to the end of the default visitors collection to 
        /// not break refined settins graph construction.
        /// </summary>
        public List<IRefinedSettingsVisitor> RefinedSettingsReadVisitors { get; set; }

        /// <summary>
        /// Field visitors to be invoked as the very first operation on the refined settings graph.
        /// Default list of the visitors used by the Eco library is defined in the InitializeRefinedSettingsSaveVisitors method.
        /// It's recomended that you append any your custom visitors to the end of the default visitors collection to 
        /// not break raw settins graph construction.
        /// </summary>
        public List<IRefinedSettingsVisitor> RefinedSettingsWriteVisitors { get; set; }

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
       /// </summary>
        public T Load<T>()
        {
            return this.Load<T>(GetDefaultSettingsFileName<T>());
        }

        /// <summary>
        /// Loads settings of the specified type from the specified file.
        /// For description of the skipNonReversableOperations argument please check overloaded version of the Load method.
        /// </summary>
        public T Load<T>(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return this.Read<T>(fileStream);
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
        public T Read<T>(Stream stream)
        {
            return (T)Read(typeof(T), stream);
        }

        /// <summary>
        /// Reads settings of the specified type from the specified TextReader.
        /// </summary>
        public T Read<T>(TextReader reader)
        {
            return (T)Read(typeof(T), reader);
        }

        /// <summary>
        /// Reads settings of the specified type from a stream.
        /// </summary>
        public object Read(Type settingsType, Stream stream)
        {
            using (var reader = new StreamReader(stream))
                return Read(settingsType, reader);
        }

        /// <summary>
        /// Reads settings of the specified type from the specified TextReader.
        /// </summary>
        public object Read(Type settingsType, TextReader reader)
        {
            return CreateRefinedSettings(settingsType, ReadRawSettings(settingsType, reader));
        }

        /// <summary>
        /// Used internally by the Eco library to read raw settings from included configuratoin files (if any).
        /// </summary>
        internal object ReadRawSettings(Type settingsType, TextReader reader)
        {
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settingsType, this.SerializationAttributesGenerator, this.DefaultUsage);
            object rawSettings = this.Serializer.Deserialize(rawSettingsType, reader);
            if (this.RawSettingsReadVisitors != null)
            {
                var visitors = this.SkipNonReversableOperations ? this.RawSettingsReadVisitors.Where(v => v.IsReversable) : this.RawSettingsReadVisitors;
                foreach (var v in visitors)
                    VisitRawFieldsRecursive(rawSettings, v);
            }
            return rawSettings;
        }

        /// <summary>
        /// Writes settings to the specified stream.
        /// </summary>
        public void Write(object settings, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
                this.Write(settings, writer);
        }

        /// <summary>
        /// Writes settings using the specified TextWriter.
        /// </summary>
        public void Write(object settings, TextWriter writer)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settings.GetType(), this.SerializationAttributesGenerator, this.DefaultUsage);
            object rawSettings = CreateRawSettings(rawSettingsType, settings);
            this.Serializer.Serialize(rawSettings, writer);
        }

        object CreateRefinedSettings(Type refinedSettingsType, object rawSettings)
        {
            object refinedSettings = Activator.CreateInstance(refinedSettingsType);
            if (this.RefinedSettingsReadVisitors != null)
            {
                var visitors = this.SkipNonReversableOperations ? this.RefinedSettingsReadVisitors.Where(v => v.IsReversable) : this.RefinedSettingsReadVisitors;
                foreach (var v in visitors)
                    VisitRefinedFieldsRecursive(refinedSettings, rawSettings, v);
            }

            return refinedSettings;
        }

        object CreateRawSettings(Type rawSettingsType, object refinedSettings)
        {
            object rawSettings = Activator.CreateInstance(rawSettingsType);
            if (this.RefinedSettingsWriteVisitors != null)
            {
                foreach (var v in this.RefinedSettingsWriteVisitors)
                    VisitRefinedFieldsRecursive(refinedSettings, rawSettings, v);
            }
            if (this.RawSettingsWriteVisitors != null)
            {
                foreach (var v in this.RawSettingsWriteVisitors)
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
                    else if (rawSettingsValue.GetType().IsSettingsOrObjectArrayType())
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
            VisitRefinedFieldsRecursive(rootRefinedSettings.GetType().Name, null, rootRefinedSettings, rawSettings, visitor, new HashSet<object>());
        }

        static void VisitRefinedFieldsRecursive(string settingsPath, string settingsNamespace, object refinedSettings, object rawSettings, IRefinedSettingsVisitor visitor, HashSet<object> visitedSettings)
        {
            if (visitedSettings.Contains(refinedSettings)) return;
            visitedSettings.Add(refinedSettings);

            foreach (var refinedSettingsField in refinedSettings.GetType().GetFields())
            {
                var rawSettingsField = rawSettings.GetType().GetField(refinedSettingsField.Name);
                string currentPath = SettingsPath.Combine(settingsPath, refinedSettingsField.Name);
                string fieldNamespace = GetFieldNamespace(refinedSettings, rawSettingsField, settingsNamespace);
                visitor.Visit(currentPath, fieldNamespace, refinedSettingsField, refinedSettings, rawSettingsField, rawSettings);

                object refinedSettingsValue = refinedSettingsField.GetValue(refinedSettings);
                object rawSettingsValue = rawSettingsField.GetValue(rawSettings);
                if (!refinedSettingsField.IsDefined<RefAttribute>() && refinedSettingsValue != null && rawSettingsValue != null)
                {
                    if (refinedSettingsValue.GetType().IsSettingsType())
                    {
                        VisitRefinedFieldsRecursive(currentPath, fieldNamespace, refinedSettingsValue, rawSettingsValue, visitor, visitedSettings);
                    }
                    else if (refinedSettingsValue.GetType().IsSettingsOrObjectArrayType())
                    {
                        var refinedSettingsArray = (Array)refinedSettingsValue;
                        var rawSettingsArray = (Array)rawSettingsValue;
                        for (int i = 0; i < refinedSettingsArray.Length; i++)
                        {
                            currentPath = SettingsPath.Combine(settingsPath, refinedSettingsField.Name, i);
                            VisitRefinedFieldsRecursive(currentPath, fieldNamespace, refinedSettingsArray.GetValue(i), rawSettingsArray.GetValue(i), visitor, visitedSettings);
                        }
                    }
                }
            }
        }


        static string GetFieldNamespace(object refinedSettings, FieldInfo refinedSettingsField, string currentNamesapce)
        {
            if (refinedSettingsField.DeclaringType.IsSubclassOf(typeof(include)))
                return SettingsPath.Combine(currentNamesapce, (refinedSettings as include).namesapce);
            else
                return currentNamesapce;
        }
}
}
