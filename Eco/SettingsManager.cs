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
        // Builds list of all ITwinSettingsTreeNodes for a single Load operation.
        readonly TwinSettingsListBuilder _refinedSettingsListBuilder = new TwinSettingsListBuilder();
        readonly HashSet<(object settings, string field)> _defaultedFields = new HashSet<(object settings, string field)>();
        readonly HashSet<(object settings, string field)> _overridenFields = new HashSet<(object settings, string field)>();

        public SettingsManager(ISerializer serializer, ISerializationAttributesGenerator serializationAttributesGenerator)
        {
            _serializer = serializer;
            _serializationAttributesGenerator = serializationAttributesGenerator;
            this.DefaultUsage = Usage.Optional;
            this.AllowUndefinedVariables = true;
            this.InitializeRawSettingsLoadVisitors();
            this.InitializeRefinedSettingsLoadVisitors();
            this.InitializeRefinedSettingsSaveVisitors();
            this.InitializeRawSettingsSaveVisitors();
        }

        void InitializeRawSettingsLoadVisitors()
        {
            var variableMapBuilder = new ConfigurationVariableMapBuilder();
            var variableExpander = new ConfigurationVariableExpander(variableMapBuilder.Variables, context: this);
            this.RawSettingsReadVisitors = new List<ISettingsVisitor>
            {
                new DefaultValueSetter(),
                variableMapBuilder,
                variableExpander,
                new EnvironmentVariableExpander(),
                new IncludeElementReader(context: this),
                new ImportElementProcessor(context: this),
                // We run ConfigurationVariableExpander twice to expand variables imported from the included files (if any).
                variableExpander,
            };
        }

        void InitializeRefinedSettingsLoadVisitors()
        {
            var settingsMapBuilder = new SettingsMapBuilder();
            this.RefinedSettingsReadVisitors = new List<ITwinSettingsVisitor>
            {
                settingsMapBuilder,
                // Use this ReferenceResolver to resolve applyDefaults.targets and applyOverrides.targets only.
                new ReferenceResolver(settingsMapBuilder.RefinedSettingsById, settingsMapBuilder.RefinedToRawMap, typeof(applyDefaults<>), typeof(applyOverrides<>)),
                // ReferenceResolver should go before ApplyDefaultsProcessor and ApplyOverridesProcessor
                // as they depend on the results produced by the former.
                new ApplyDefaultsProcessor(settingsMapBuilder.RefinedSettingsById, settingsMapBuilder.RefinedToRawMap),
                new ApplyOverridesProcessor(settingsMapBuilder.RefinedSettingsById, settingsMapBuilder.RefinedToRawMap),
                new FieldReferenceExpander(),
                // Include ReferenceResolver for the second time to resolve the rest references.
                new ReferenceResolver(settingsMapBuilder.RefinedSettingsById, settingsMapBuilder.RefinedToRawMap),
                // RefList modifications are applied to the refined settings only, 
                // thus they need to be processed only after all references get resolved.
                new RefListModificationProcessor(settingsMapBuilder.RefinedSettingsById),
                new RequiredFieldChecker()
            };

            // Link IDynamicSettingsConstuctor(s) and IDynamicSettingsConstructorObserver(s)
            var readVisitors = this.RefinedSettingsReadVisitors.Append(_refinedSettingsListBuilder);
            foreach (var observer in readVisitors.OfType<ISettingsVisitorObserver>())
            {
                foreach (var v in readVisitors)
                    observer.Observe(v);
            }
        }

        void InitializeRawSettingsSaveVisitors()
        {
            this.RawSettingsWriteVisitors = new List<ISettingsVisitor>
            {
                new IncludeElementWriter(this)
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
        /// 
        /// By default DefaultUsage is set to Optional.
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
        /// If AllowUndefinedVariables is set to false, Eco would throw an exception if it matches undefined configuration variable.
        /// Otherwise, variable is expanded to an empty string.
        /// 
        /// By default AllowUndefinedVariables is set to true.
        /// </summary>
        public bool AllowUndefinedVariables { get; set; }

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

        ///// <summary>
        ///// RefinedSettingsListBuilder is used internally by SettingsManager to dump all twin settings to a list
        ///// and use the list further in place of traversing the settings tree through the reflection.
        ///// If you modify the default list of RefinedSettingsReadVisitors, you might need to link RefinedSettingsListBuilder
        ///// to the added IDynamicSettingsConstructor(s), if any.
        ///// </summary>
        public ISettingsVisitorObserver RefinedSettingsListBuilder => _refinedSettingsListBuilder;

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
        /// </summary>.
        public object Read(Type settingsType, TextReader reader)
        {
            var readVisitors = this.RawSettingsReadVisitors.OfType<object>().Concat(this.RefinedSettingsReadVisitors);
            using (DefaultsCheck(readVisitors))
            using (OverridesCheck(readVisitors))
            {
                Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settingsType, this.Serializer, this.SerializationAttributesGenerator, this.DefaultUsage);
                var rawSettings = ReadRawSettings(
                    currentNamespace: null,
                    currentSettingsPath: rawSettingsType.Name,
                    rawSettingsType: rawSettingsType,
                    reader: reader,
                    initializeVisitors: true);
                return CreateRefinedSettings(settingsType, rawSettings);
            }
        }

        static IDisposable DefaultsCheck(IEnumerable<object> visitors)
        {
            var defaultedFields = new HashSet<(object settings, string field)>();
            //var readVisitors = this.RawSettingsReadVisitors.OfType<object>().Concat(this.RefinedSettingsReadVisitors);
            foreach (var v in visitors.OfType<IDefaultValueSetter>())
                v.InitializingField += OnFieldInitialized;

            return new Disposable(() =>
            {
                foreach (var v in visitors.OfType<IDefaultValueSetter>())
                    v.InitializingField -= OnFieldInitialized;
            });

            void OnFieldInitialized((object settings, string field) fieldInfo)
            {
                if (defaultedFields.Contains(fieldInfo)) throw new ConfigurationException($"Second time initialization: field={fieldInfo.settings.GetType().Name}.{fieldInfo.field}");
                defaultedFields.Add(fieldInfo);
            }
        }

        static IDisposable OverridesCheck(IEnumerable<object> visitors)
        {
            var overridenFields = new HashSet<(object settings, string field)>();
            //var readVisitors = this.RawSettingsReadVisitors.OfType<object>().Concat(this.RefinedSettingsReadVisitors);
            foreach (var v in visitors.OfType<IFieldValueOverrider>())
                v.OverridingField += OnFieldOverriden;

            return new Disposable(() =>
            {
                foreach (var v in visitors.OfType<IDefaultValueSetter>())
                    v.InitializingField -= OnFieldOverriden;
            });

            void OnFieldOverriden((object settings, string field) fieldInfo)
            {
                if (overridenFields.Contains(fieldInfo)) throw new ConfigurationException($"Second time override: field={fieldInfo.settings.GetType().Name}.{fieldInfo.field}");
                overridenFields.Add(fieldInfo);
            }
        }

        /// <summary>
        /// Used internally by the Eco library to read raw settings from included configuratoin files (if any).
        /// </summary>
        internal object ReadRawSettings(string currentNamespace, string currentSettingsPath, Type rawSettingsType, TextReader reader, bool initializeVisitors)
        {
            object rawSettings = this.Serializer.Deserialize(rawSettingsType, reader);
            InitilizeRawSettings(currentNamespace, currentSettingsPath, rawSettings, initializeVisitors);
            return rawSettings;
        }

        /// <summary>
        /// Used internally by the Eco library to initialize raw settings included/imported from sub-configuratoin files (if any).
        /// </summary>
        internal void InitilizeRawSettings(string currentNamespace, string currentSettingsPath, object rawSettings, bool initializeVisitors)
        {
            if (this.RawSettingsReadVisitors != null)
            {
                var settingsListBuilder = new SettingsListBuilder();
                TraverseSeetingsTree(
                    startNamespace: currentNamespace,
                    startPath: currentSettingsPath,
                    rootMasterSettings: rawSettings,
                    visitor: settingsListBuilder,
                    initVisitor: initializeVisitors);

                // TraverseTwinSeetingsTrees uses Reflection to go through the settings tree.
                // Below we dump all tree nodes to a list and use it further instead of traversing the tree through the reflection.
                var visitors = this.SkipNonReversableOperations ? this.RawSettingsReadVisitors.Where(v => v.IsReversable) : this.RawSettingsReadVisitors;
                foreach (var v in visitors)
                {
                    if (initializeVisitors)
                        v.Initialize(rootSettingsType: rawSettings.GetType());

                    var settings = settingsListBuilder.Settings;
                    for (int i = 0; i < settings.Count; i++)
                    {
                        var node = settingsListBuilder.Settings[i];
                        if (node.SkippedBy(v))
                        {
                            // Skip all child nodes. (all children goes right after parent)
                            while (i < settings.Count && settings[i].Path.StartsWith(node.Path))
                                i++;

                            node = i < settings.Count ? settings[i] : null;
                        }
                        node?.Accept(v);
                    }
                }
            }
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
            Type rawSettingsType = SerializableTypeEmitter.GetRawTypeFor(settings.GetType(), this.Serializer, this.SerializationAttributesGenerator, this.DefaultUsage);
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
            var mandatoryVisitors = new ITwinSettingsVisitor[]
            {
                new RefinedSettingsBuilder(),
                _refinedSettingsListBuilder
            };
            foreach (var v in mandatoryVisitors)
            {
                TraverseTwinSeetingsTrees(
                    startNamespace: null,
                    startPath: refinedSettingsType.Name,
                    rootMasterSettings: refinedSettings,
                    rootSlaveSettings: rawSettings,
                    visitor: v);
            }
            if (this.RefinedSettingsReadVisitors != null)
            {
                // TraverseTwinSeetingsTrees uses Reflection to go through the settings tree.
                // Below we use tree nodes dumped to a list instead of traversing the tree through the reflection.
                var userVisitors = this.SkipNonReversableOperations ? this.RefinedSettingsReadVisitors.Where(v => v.IsReversable) : this.RefinedSettingsReadVisitors;
                foreach (var v in userVisitors)
                {
                    v.Initialize(rootMasterSettingsType: refinedSettings.GetType(), rootSlaveSettingsType: rawSettings.GetType());
                    var settings = _refinedSettingsListBuilder.Settings;
                    for (int i = 0; i < settings.Count; i++)
                    {
                        var node = settings[i];
                        if (node.SkippedBy(v))
                        {
                            // Skip all child nodes. (all children goes right after parent)
                            while (i < settings.Count && settings[i].Path.StartsWith(node.Path))
                                i++;

                            node = i < settings.Count ? settings[i] : null;
                        }
                        node?.Accept(v);
                    }
                }
            }

            return refinedSettings;
        }

        object CreateRawSettings(Type rawSettingsType, object refinedSettings)
        {
            object rawSettings = Activator.CreateInstance(rawSettingsType);
            if (this.RefinedSettingsWriteVisitors != null)
            {
                foreach (var v in this.RefinedSettingsWriteVisitors)
                    TraverseTwinSeetingsTrees(
                        startNamespace: null,
                        startPath: refinedSettings.GetType().Name,
                        rootMasterSettings: refinedSettings,
                        rootSlaveSettings: rawSettings,
                        visitor: v,
                        initVisitor: true);
            }
            if (this.RawSettingsWriteVisitors != null)
            {
                foreach (var v in this.RawSettingsWriteVisitors)
                {
                    TraverseSeetingsTree(
                        startNamespace: null,
                        startPath: refinedSettings.GetType().Name,
                        rootMasterSettings: rawSettings,
                        visitor: v,
                        initVisitor: true);
                }
            }

            return rawSettings;
        }

        public static void TraverseSeetingsTree(
            string startNamespace,
            string startPath,
            object rootMasterSettings,
            ISettingsVisitor visitor,
            bool initVisitor = true,
            Func<FieldInfo, object, bool> SkipBranch = null)
        {
            Func<FieldInfo, object, bool> DefaultSkipBranch = (f, o) => false;
            if (initVisitor) visitor.Initialize(rootMasterSettings.GetType());
            TraverseTwinSeetingsTreesRecursive(
                currentNamespace: startNamespace,
                settingsPath: startPath,
                masterSettings: rootMasterSettings,
                slaveSettings: null,
                visitorType: visitor.GetType(),
                VisitTwinSettings: (currentNamespace, settingsPath, masterSettings, slaveSettings) => visitor.Visit(currentNamespace, settingsPath, masterSettings),
                VisitTwinSettingsField: (settingsNamespace, fieldPath, masterSettings, masterSettingsField, slaveSettings, slaveSettingsField) => visitor.Visit(settingsNamespace, fieldPath, masterSettings, masterSettingsField),
                SkipBranch: SkipBranch ?? DefaultSkipBranch);
        }

        public static void TraverseTwinSeetingsTrees(
            string startNamespace,
            string startPath,
            object rootMasterSettings,
            object rootSlaveSettings,
            ITwinSettingsVisitor visitor,
            bool initVisitor = true,
            Func<FieldInfo, object, bool> SkipBranch = null)
        {
            Func<FieldInfo, object, bool> DefaultSkipBranch = (f, o) => false;
            if (initVisitor) visitor.Initialize(rootMasterSettings.GetType(), rootSlaveSettings.GetType());
            TraverseTwinSeetingsTreesRecursive(
                currentNamespace: startNamespace,
                settingsPath: startPath,
                masterSettings: rootMasterSettings,
                slaveSettings: rootSlaveSettings,
                visitorType: visitor.GetType(),
                VisitTwinSettings: visitor.Visit,
                VisitTwinSettingsField: visitor.Visit,
                SkipBranch: SkipBranch ?? DefaultSkipBranch);
        }

        delegate void VisitTwinSettingsDelegate(string currentNamespace, string settingsPath, object masterSettings, object slaveSettings);
        delegate void VisitTwinSettingsFieldDelegate(string settingsNamespace, string fieldPath, object masterSettings, FieldInfo masterSettingsField, object slaveSettings, FieldInfo slaveSettingsField);

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

                string currentPath = SettingsPath.Combine(left: settingsPath, right: masterField.Name);
                VisitTwinSettingsField(fieldNamespace, currentPath, masterSettings, masterField, slaveSettings, slaveField);

                object masterValue = masterField.GetValue(masterSettings);
                object slaveValue = slaveField?.GetValue(slaveSettings);
                if (!masterField.IsDefined<RefAttribute>() && masterValue != null && !SkipBranch(masterField, masterSettings))
                {
                    if (masterValue.GetType().IsSettingsType())
                    {
                        string ns = SettingsPath.Combine(currentNamespace, localNamespace);
                        currentPath = SettingsPath.AddType(currentPath, objectType: masterValue.GetType().GetCsharpCompatibleName());
                        TraverseTwinSeetingsTreesRecursive(fieldNamespace, currentPath, masterValue, slaveValue, visitorType, VisitTwinSettings, VisitTwinSettingsField, SkipBranch);
                    }
                    else if (masterValue.GetType().IsSettingsOrObjectArrayType())
                    {
                        var masterArray = (Array)masterValue;
                        var slaveArray = (Array)slaveValue;
                        for (int i = 0; i < masterArray.Length; i++)
                        {
                            var itemValue = masterArray.GetValue(i);
                            string itemPath = SettingsPath.AddType(currentPath, objectType: itemValue.GetType().GetCsharpCompatibleName(), index: i);
                            TraverseTwinSeetingsTreesRecursive(fieldNamespace, itemPath, itemValue, slaveArray?.GetValue(i), visitorType, VisitTwinSettings, VisitTwinSettingsField, SkipBranch);
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
