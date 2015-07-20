using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
			return typeof(T).Name + ".config";
		}

		public T Load<T>(bool skipPostProcessing = false) where T : new()
		{
			return this.Load<T>(GetDefaultSettingsFileName<T>(), skipPostProcessing);
		}

		public T Load<T>(string fileName, bool skipPostProcessing = false) where T : new()
		{
			using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				return this.Read<T>(fileStream, skipPostProcessing);
		}

		public void Save<T>(T settings)
		{
			this.Save(GetDefaultSettingsFileName<T>());
		}

		public void Save<T>(T settings, string fileName) 
		{
			using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
				this.Write(settings, fileStream);
		}

		public T Read<T>(Stream stream, bool skipPostProcessing = false) where T : new()
        {
			Type rawSettingsType = SerializableTypeEmitter.GetSerializableTypeFor<T>(this.SerializationAttributesGenerator, this.DefaultUsage);
			object rawSettings = this.Serializer.Deserialize(rawSettingsType, stream);
            T refinedSettings = new T();
			VisitAllFieldsRecursive(sourceSettings: rawSettings, targetSettings: refinedSettings, visitor: new SettingsObjectBuilder());
			var settingsMapBuilder = new SettingsMapBuilder();
			VisitAllFieldsRecursive(sourceSettings: rawSettings, targetSettings: refinedSettings, visitor: settingsMapBuilder);
			VisitAllFieldsRecursive(sourceSettings: rawSettings, targetSettings: refinedSettings, visitor: new ReferenceResolver(settingsMapBuilder.SettingsByIdMap));
			VisitAllFieldsRecursive(sourceSettings: rawSettings, targetSettings: refinedSettings, visitor: new RequiredFieldChecker());
			if (!skipPostProcessing)
			{
				VisitAllFieldsRecursive(sourceSettings: rawSettings, targetSettings: refinedSettings, visitor: new EnvironmentVariableExpander());
				foreach (var customVisitor in _customLoadVisitors)
					VisitAllFieldsRecursive(sourceSettings: rawSettings, targetSettings: refinedSettings, visitor: customVisitor);
			}

			return refinedSettings;
        }

        public void Write<T>(T settings, Stream stream)
        {
			Type rawSettingsType = SerializableTypeEmitter.GetSerializableTypeFor<T>(this.SerializationAttributesGenerator, this.DefaultUsage);
			object rawSettings = Activator.CreateInstance(rawSettingsType);
			VisitAllFieldsRecursive(sourceSettings: settings, targetSettings: rawSettings, visitor: new SettingsObjectBuilder());
			VisitAllFieldsRecursive(sourceSettings: settings, targetSettings: rawSettings, visitor: new ReferencePacker());
			VisitAllFieldsRecursive(sourceSettings: settings, targetSettings: rawSettings, visitor: new RequiredFieldChecker());
			foreach (var customVisitor in _customSaveVisitors)
				VisitAllFieldsRecursive(sourceSettings: settings, targetSettings: rawSettings, visitor: new RequiredFieldChecker());

			this.Serializer.Serialize(rawSettings, stream);
		}

		static void VisitAllFieldsRecursive(object sourceSettings, object targetSettings, IFieldVisitor visitor)
		{
			VisitAllFieldsRecursive(sourceSettings.GetType().Name, sourceSettings, targetSettings, visitor, new HashSet<object>());
        }

		static void VisitAllFieldsRecursive(string settingsPath, object sourceSettings, object targetSettings, IFieldVisitor visitor, HashSet<object> visitedSettings)
		{
			if (visitedSettings.Contains(sourceSettings)) return;
			visitedSettings.Add(sourceSettings);

			foreach (var sourceSettingsField in sourceSettings.GetType().GetFields())
			{
				var targetSettingsField = targetSettings.GetType().GetField(sourceSettingsField.Name);
				string currentPath = SettingsPath.Combine(settingsPath, sourceSettingsField.Name);
                visitor.Visit(currentPath, sourceSettingsField, sourceSettings, targetSettingsField, targetSettings);

				object sourceSettingsValue = sourceSettingsField.GetValue(sourceSettings);
				object targetSettingsValue = targetSettingsField.GetValue(targetSettings);
				if (sourceSettingsValue != null && targetSettingsValue != null && 
					!sourceSettingsField.IsDefined<RefAttribute>() && !targetSettingsField.IsDefined<RefAttribute>())
				{
					if (sourceSettingsValue.GetType().IsSettingsType())
					{
						VisitAllFieldsRecursive(currentPath, sourceSettingsValue, targetSettingsValue, visitor, visitedSettings);
					}
					else if (sourceSettingsValue.GetType().IsSettingsArrayType())
					{
						var sourceSettingsArray = (Array)sourceSettingsValue;
						var targetSettingsArray = (Array)targetSettingsValue;
						for (int i = 0; i < sourceSettingsArray.Length; i++)
						{
							currentPath = SettingsPath.Combine(settingsPath, sourceSettingsField.Name, i);
                            VisitAllFieldsRecursive(currentPath, sourceSettingsArray.GetValue(i), targetSettingsArray.GetValue(i), visitor, visitedSettings);
						}
					}
				}
			}
		}
    }
}
