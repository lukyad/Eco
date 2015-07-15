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
		readonly ISerializaer _serializer;
		readonly ISerializationAttributesGenerator _serializationAttributesGenerator;

        public SettingsManager(ISerializaer serializer, ISerializationAttributesGenerator serializationAttributesGenerator)
		{
			_serializer = serializer;
			_serializationAttributesGenerator = serializationAttributesGenerator;
        }

		public ISerializaer Serializer { get { return _serializer; } }

		public ISerializationAttributesGenerator SerializationAttributesGenerator { get { return _serializationAttributesGenerator; } }

		public Usage DefaultUsage { get; set; }

		public static SettingsManager Default = new SettingsManager(new XmlSerializer(), new XmlAttributesGenerator());

		public T Load<T>()
		{
			return default(T);
		}

		public T Load<T>(string fileName)
		{
			return default(T);
		}

		public void Save<T>(T settings)
		{

		}

		public void Save<T>(T settings, string fileName)
		{
		}

		public T Read<T>(Stream stream) where T : new()
        {
			Type rawSettingsType = SerializableTypeEmitter.EmitSerializableTypeFor<T>(this.SerializationAttributesGenerator, this.DefaultUsage);
			object rawSettings = this.Serializer.Deserialize(rawSettingsType, stream);
            T refinedSettings = new T();
			VisitAllFieldsRecursive(sourceSettings: rawSettings, targetSettings: refinedSettings, visitor: new SettingsObjectBuilder());
			var settingsMapBuilder = new SettingsMapBuilder();
			VisitAllFieldsRecursive(sourceSettings: rawSettings, targetSettings: refinedSettings, visitor: settingsMapBuilder);
			VisitAllFieldsRecursive(sourceSettings: rawSettings, targetSettings: refinedSettings, visitor: new ReferenceResolver(settingsMapBuilder.SettingsByIdMap));
            return refinedSettings;
        }

        public void Write<T>(T refinedSettings, Stream stream)
        {
			Type rawSettingsType = SerializableTypeEmitter.EmitSerializableTypeFor<T>(this.SerializationAttributesGenerator, this.DefaultUsage);
			object rawSettings = Activator.CreateInstance(rawSettingsType);
			VisitAllFieldsRecursive(sourceSettings: refinedSettings, targetSettings: rawSettings, visitor: new SettingsObjectBuilder());
			VisitAllFieldsRecursive(sourceSettings: refinedSettings, targetSettings: rawSettings, visitor: new ReferencePacker());
			this.Serializer.Serialize(rawSettings, stream);
		}

		static void VisitAllFieldsRecursive(object sourceSettings, object targetSettings, IFieldVisitor visitor)
		{
			VisitAllFieldsRecursive(sourceSettings, targetSettings, visitor, new HashSet<object>());
        }

		static void VisitAllFieldsRecursive(object sourceSettings, object targetSettings, IFieldVisitor visitor, HashSet<object> visitedSettings)
		{
			if (visitedSettings.Contains(sourceSettings)) return;
			visitedSettings.Add(sourceSettings);

			foreach (var sourceSettingsField in sourceSettings.GetType().GetFields())
			{
				var targetSettingsField = targetSettings.GetType().GetField(sourceSettingsField.Name);
				visitor.Visit(sourceSettingsField, sourceSettings, targetSettingsField, targetSettings);

				object sourceSettingsValue = sourceSettingsField.GetValue(sourceSettings);
				object targetSettingsValue = targetSettingsField.GetValue(targetSettings);
				if (sourceSettingsValue != null && targetSettingsValue != null && 
					!sourceSettingsField.IsDefined<RefAttribute>() && !targetSettingsField.IsDefined<RefAttribute>())
				{
					if (sourceSettingsValue.GetType().IsSettingsType())
					{
						VisitAllFieldsRecursive(sourceSettingsValue, targetSettingsValue, visitor, visitedSettings);
					}
					else if (sourceSettingsValue.GetType().IsSettingsArrayType())
					{
						var sourceSettingsArray = (Array)sourceSettingsValue;
						var targetSettingsArray = (Array)targetSettingsValue;
						for (int i = 0; i < sourceSettingsArray.Length; i++)
							VisitAllFieldsRecursive(sourceSettingsArray.GetValue(i), targetSettingsArray.GetValue(i), visitor, visitedSettings);
					}
				}
			}
		}

		static IEnumerable<object> GetSettingsRecursively(object root)
		{
			return GetSettingsRecursively(root, new HashSet<object>());
        }

        static IEnumerable<object> GetSettingsRecursively(object settings, HashSet<object> visitedSettings)
		{
            if (settings == null || visitedSettings.Contains(settings)) yield break;
			visitedSettings.Add(settings);
			yield return settings;

            foreach (var field in settings.GetType().GetFields())
            {
                object fieldValue = field.GetValue(settings);
                if (fieldValue != null)
                {
                    if (fieldValue.GetType().IsSettingsType())
                    {
                        foreach (var s in GetSettingsRecursively(fieldValue))
                            yield return s;
                    }
                    else if (fieldValue.GetType().IsSettingsArrayType())
                    {
                        var settingsArray = (object[])fieldValue;
                        foreach (var item in settingsArray)
                        {
                            foreach (var s in GetSettingsRecursively(item))
                                yield return s;
                        }
                    }
                }
            }
        }
    }
}
