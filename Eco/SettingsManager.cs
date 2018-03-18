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
            this.DefaultUsage = Usage.Optional;
            this.InitializeRawSettingsLoadVisitors();
            this.InitializeRefinedSettingsLoadVisitors();
            this.InitializeRefinedSettingsSaveVisitors();
            this.InitializeRawSettingsSaveVisitors();
        }

        void InitializeRawSettingsLoadVisitors()
        {
            var variableMapBuilder = new ConfigurationVariableMapBuilder();
            var variableExpander = new ConfigurationVariableExpander(variableMapBuilder.Variables);
            this.RawSettingsReadVisitors = new List<ISettingsVisitor>
            {
                new DefaultValueSetter(),
                variableMapBuilder,
                variableExpander,
                new EnvironmentVariableExpander(),
                new IncludeElementReader(this),
                new ImportElementReader(),
                // We run ConfigurationVariableExpander twice to expand variables from the included files (if any).
                variableExpander
            };
        }

        void InitializeRefinedSettingsLoadVisitors()
        {
            var settingsMapBuilder = new SettingsMapBuilder();
            var referenceResolver = new ReferenceResolver(settingsMapBuilder.RefinedSettingsById, settingsMapBuilder.RefinedToRawMap);
            var defaultedAndOverridenFields = new HashSet<Tuple<object, FieldInfo>>();
            this.RefinedSettingsReadVisitors = new List<ITwinSettingsVisitor>
            {
                new RefinedSettingsBuilder(),
                settingsMapBuilder,
                referenceResolver,
                // ReferenceResolver should go before ApplyDefaultsProcessor and ApplyOverridesProcessor
                // as they depend on the results produced by the former.
                new ApplyDefaultsProcessor(settingsMapBuilder.RefinedSettingsById, settingsMapBuilder.RefinedToRawMap, /*out*/ defaultedAndOverridenFields),
                new ApplyOverridesProcessor(settingsMapBuilder.RefinedSettingsById, settingsMapBuilder.RefinedToRawMap, /*out*/ defaultedAndOverridenFields),
                new FieldReferenceExpander(),
                // Include ReferenceResolver for the second time as StringFieldReferenceExpander could substitute additional valid references.
                referenceResolver, 
                new RequiredFieldChecker(defaultedAndOverridenFields)
            };
        }

        void InitializeRawSettingsSaveVisitors()
        {
            this.RawSettingsWriteVisitors = new List<ISettingsVisitor>
            {
                new IncludeElementWriter(this),
                new ImportElementWriter()
            };
        }

        void InitializeRefinedSettingsSaveVisitors()
        {
            var namespaceMapBuilder = new NamespaceMapBuilder();
            this.RefinedSettingsWriteVisitors = new List<ITwinSettingsVisitor>
            {
                new RawSettingsBuilder(),
                namespaceMapBuilder,
                new ReferencePacker(namespaceMapBuilder.NamespaceMap),
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
        /// Thus, the result file would not be equivalent to the original one.
        /// If you are planning to save settings back to the file and want to keep the orginal 'features' of the file, set the SkipNonReversableOperations to true.
        /// It would instruct Eco library to skip any field visitors that does non-reversable changes.
        /// 
        /// An example of a visitor that makes non-reversable changes is the EnvironmentVariableExpander visitor.
        /// Once expanded, variables could not be collapsed back and if loaded in another environment, 
        /// configuration would not be equivalent to the original one.
        /// 
        /// By default SkipNonReversableOperations is set to false.
        public bool SkipNonReversableOperations { get; set; }

        /// <summary>
        /// Fields visitors to be invoked right after raw settins object has been deserialized from a source stream.
        /// By default Eco library uses ConfigurationVariableExpander and EnvironmentVariableExpander visitors.
        /// </summary>
        public List<ISettingsVisitor> RawSettingsReadVisitors { get; set; }

        /// <summary>
        /// Fields visitors to be invoked right before raw settins object to be serialized to a stream.
        /// Eco library doesn't implement any default save visitors.
        /// </summary>
        public List<ISettingsVisitor> RawSettingsWriteVisitors { get; set; }

        /// <summary>
        /// Field visitors to be invoked after raw settins load visitors complete thier job.
        /// Default list of the visitors used by the Eco library is defined in the InitializeRefinedSettingsLoadVisitors method.
        /// It's recomended that you append any your custom visitors to the end of the default visitors collection to 
        /// not break refined settins graph construction.
        /// </summary>
        public List<ITwinSettingsVisitor> RefinedSettingsReadVisitors { get; set; }

        /// <summary>
        /// Field visitors to be invoked as the very first operation on the refined settings graph.
        /// Default list of the visitors used by the Eco library is defined in the InitializeRefinedSettingsSaveVisitors method.
        /// It's recomended that you append any your custom visitors to the end of the default visitors collection to 
        /// not break raw settins graph construction.
        /// </summary>
        public List<ITwinSettingsVisitor> RefinedSettingsWriteVisitors { get; set; }

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
            using (var fileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
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
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settingsType, this.SerializationAttributesGenerator, this.DefaultUsage);
            return CreateRefinedSettings(settingsType, ReadRawSettings(rawSettingsType, reader, initializeVisitors: true));
        }

        /// <summary>
        /// Used internally by the Eco library to read raw settings from included configuratoin files (if any).
        /// </summary>
        internal object ReadRawSettings(Type rawSettingsType, TextReader reader, bool initializeVisitors)
        {
            object rawSettings = this.Serializer.Deserialize(rawSettingsType, reader);
            if (this.RawSettingsReadVisitors != null)
            {
                var visitors = this.SkipNonReversableOperations ? this.RawSettingsReadVisitors.Where(v => v.IsReversable) : this.RawSettingsReadVisitors;
                Func<FieldInfo, object, bool> IsInsideIncludeElement = (f, o) => f.DeclaringType.IsGenericType && f.DeclaringType.GetGenericTypeDefinition() == typeof(include<>);
                foreach (var v in visitors)
                    TraverseSeetingsTree(rawSettings, v, initializeVisitors,  SkipBranch: IsInsideIncludeElement);
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
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settings.GetType(), this.SerializationAttributesGenerator, this.DefaultUsage);
            object rawSettings = CreateRawSettings(rawSettingsType, settings);
            this.Serializer.Serialize(rawSettings, writer);
        }

        /// <summary>
        /// Given a root settings object, enumerate all child settings objects recursive.
        /// </summary>
        public static IEnumerable<object> EnumerateSettings(object root)
        {
            return EnumerateSettingsRecursive(root);
        }

        static IEnumerable<object> EnumerateSettingsRecursive(object current)
        {
            if (current == null || !current.GetType().IsSettingsType()) yield break;
            yield return current;

            foreach (var field in current.GetType().GetFields().Where(f => !f.IsDefined<RefAttribute>()))
            {
                object fieldValue = field.GetValue(current);
                if (fieldValue != null)
                {
                    if (fieldValue.GetType().IsSettingsType())
                    {
                        foreach (var child in EnumerateSettingsRecursive(field.GetValue(current)))
                            yield return child;
                    }
                    else if (fieldValue.GetType().IsSettingsOrObjectArrayType())
                    {
                        var array = (Array)fieldValue;
                        for (int i = 0; i < array.Length; i++)
                        {
                            foreach (var child in EnumerateSettingsRecursive(array.GetValue(i)))
                                yield return child;
                        }
                    }
                }
            }
        }

        object CreateRefinedSettings(Type refinedSettingsType, object rawSettings)
        {
            object refinedSettings = Activator.CreateInstance(refinedSettingsType);
            if (this.RefinedSettingsReadVisitors != null)
            {
                var visitors = this.SkipNonReversableOperations ? this.RefinedSettingsReadVisitors.Where(v => v.IsReversable) : this.RefinedSettingsReadVisitors;
                foreach (var v in visitors)
                    TraverseTwinSeetingsTrees(refinedSettings, rawSettings, v);
            }

            return refinedSettings;
        }

        object CreateRawSettings(Type rawSettingsType, object refinedSettings)
        {
            object rawSettings = Activator.CreateInstance(rawSettingsType);
            if (this.RefinedSettingsWriteVisitors != null)
            {
                foreach (var v in this.RefinedSettingsWriteVisitors)
                    TraverseTwinSeetingsTrees(refinedSettings, rawSettings, v, initVisitor: true);
            }
            if (this.RawSettingsWriteVisitors != null)
            {
                foreach (var v in this.RawSettingsWriteVisitors)
                    TraverseSeetingsTree(rawSettings, v, initVisitor: true);
            }
           
            return rawSettings;
        }

        public static void TraverseSeetingsTree(object rootMasterSettings, ISettingsVisitor visitor, bool initVisitor = true, Func<FieldInfo, object, bool> SkipBranch = null)
        {
            Func<FieldInfo, object, bool>  DefaultSkipBranch = (f, o) => false;
            if (initVisitor) visitor.Initialize(rootMasterSettings.GetType());
            TraverseTwinSeetingsTreesRecursive(
                currentNamespace: null,
                settingsPath: rootMasterSettings.GetType().Name,
                masterSettings: rootMasterSettings,
                slaveSettings: null,
                visitorType: visitor.GetType(),
                VisitTwinSettings: (currentNamespace, settingsPath, masterSettings, slaveSettings) => visitor.Visit(currentNamespace, settingsPath, masterSettings),
                VisitTwinSettingsField: (settingsNamespace, fieldPath, masterSettingsField, masterSettings, slaveSettingsField, slaveSettings) => visitor.Visit(settingsNamespace, fieldPath, masterSettingsField, masterSettings),
                SkipBranch: SkipBranch ?? DefaultSkipBranch);
        }

        public static void TraverseTwinSeetingsTrees(object rootMasterSettings, object rootSlaveSettings, ITwinSettingsVisitor visitor, bool initVisitor = true, Func<FieldInfo, object, bool> SkipBranch = null)
        {
            Func<FieldInfo, object, bool> DefaultSkipBranch = (f, o) => false;
            if (initVisitor) visitor.Initialize(rootMasterSettings.GetType(), rootSlaveSettings.GetType());
            TraverseTwinSeetingsTreesRecursive(
                currentNamespace: null,
                settingsPath: rootMasterSettings.GetType().Name,
                masterSettings: rootMasterSettings,
                slaveSettings: rootSlaveSettings,
                visitorType: visitor.GetType(),
                VisitTwinSettings: visitor.Visit,
                VisitTwinSettingsField: visitor.Visit,
                SkipBranch: SkipBranch ?? DefaultSkipBranch);
        }

        delegate void VisitTwinSettingsDelegate(string currentNamespace, string settingsPath, object masterSettings, object slaveSettings);
        delegate void VisitTwinSettingsFieldDelegate(string settingsNamespace, string fieldPath, FieldInfo masterSettingsField, object masterSettings, FieldInfo slaveSettingsField, object slaveSettings);

        // Traverses two twin settings trees.
        // Allows slave tree to be null.
        static void TraverseTwinSeetingsTreesRecursive(
            string currentNamespace, 
            string settingsPath, 
            object masterSettings, 
            object slaveSettings,
            Type visitorType,
            VisitTwinSettingsDelegate VisitTwinSettings,
            VisitTwinSettingsFieldDelegate VisitTwinSettingsField,
            Func<FieldInfo, object, bool> SkipBranch)
        {
            VisitTwinSettings(currentNamespace, settingsPath, masterSettings, slaveSettings);
            string localNamespace = GetLocalNamespace(masterSettings);
            foreach (var masterField in masterSettings.GetType().GetPublicInstanceFields())
            {
                HashSet<Type> skippedByVisitors = masterField.GetCustomAttribute<SkippedByAttribute>()?.Visitors;
                if (skippedByVisitors != null && skippedByVisitors.Contains(visitorType)) continue;

                var slaveField = slaveSettings?.GetType().GetField(masterField.Name);
                string fieldNamespace = localNamespace == null || masterField.FieldType.IsSimple() ? currentNamespace : SettingsPath.Combine(currentNamespace, localNamespace);
                string currentPath = SettingsPath.Combine(settingsPath, masterField.Name);
                VisitTwinSettingsField(fieldNamespace, currentPath, masterField, masterSettings, slaveField, slaveSettings);

                object masterValue = masterField.GetValue(masterSettings);
                object slaveValue = slaveField?.GetValue(slaveSettings);
                if (!masterField.IsDefined<RefAttribute>() && masterValue != null && !SkipBranch(masterField, masterSettings))
                {
                    if (masterValue.GetType().IsSettingsType())
                    {
                        string ns = SettingsPath.Combine(currentNamespace, localNamespace);
                        TraverseTwinSeetingsTreesRecursive(fieldNamespace, currentPath, masterValue, slaveValue, visitorType, VisitTwinSettings, VisitTwinSettingsField, SkipBranch);
                    }
                    else if (masterValue.GetType().IsSettingsOrObjectArrayType())
                    {
                        var masterArray = (Array)masterValue;
                        var slaveArray = (Array)slaveValue;
                        for (int i = 0; i < masterArray.Length; i++)
                        {
                            currentPath = SettingsPath.Combine(settingsPath, masterField.Name, i);
                            TraverseTwinSeetingsTreesRecursive(fieldNamespace, currentPath, masterArray.GetValue(i), slaveArray?.GetValue(i), visitorType, VisitTwinSettings, VisitTwinSettingsField, SkipBranch);
                        }
                    }
                }
            }
        }

        static string GetLocalNamespace(object settings)
        {
            return (string)settings.GetType().GetFields().FirstOrDefault(f => f.IsDefined<NamespaceAttribute>())?.GetValue(settings);
        }
}
}
