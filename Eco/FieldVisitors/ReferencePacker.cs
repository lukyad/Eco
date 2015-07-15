using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco
{
	class ReferencePacker : IFieldVisitor
	{
		public void Visit(FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
		{
			if (refinedSettingsField.IsDefined<RefAttribute>())
			{
				if (refinedSettingsField.FieldType.IsSettingsType()) PackReference(refinedSettingsField, refinedSettings, rawSettingsField, rawSettings);
				else if (refinedSettingsField.FieldType.IsSettingsArrayType()) PackReferenceArray(refinedSettingsField, refinedSettings, rawSettingsField, rawSettings);
				else throw new ApplicationException("Did not expect to get here");
			}
		}

		static void PackReference(FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
		{
			object settings = refinedSettingsField.GetValue(refinedSettings);
			if (settings == null) return;
			rawSettingsField.SetValue(rawSettings, GetSettingsId(settings));
		}

		static void PackReferenceArray(FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
		{
			Array settingsArray = (Array)refinedSettingsField.GetValue(refinedSettings);
            if (settingsArray == null) return;
			var referenceListBuilder = new StringBuilder();
			for (int i = 0; i < settingsArray.Length; i++)
			{
				var settings = settingsArray.GetValue(i);
				if (settings != null)
				{
					string id = GetSettingsId(settings);
					referenceListBuilder.Append(id + Settings.IdSeparator);
                }
            }
			string referenceList = referenceListBuilder.ToString().TrimEnd(Settings.IdSeparator);
			rawSettingsField.SetValue(rawSettings, String.IsNullOrEmpty(referenceList) ? null : referenceList);
        }

		static string GetSettingsId(object settings)
		{
			FieldInfo idField = settings.GetType().GetFields().SingleOrDefault(f => f.IsDefined<IdAttribute>());
			if (idField == null)
				throw new ApplicationException(String.Format("Expected an object with one of the fields marked with {0}, but got an instance of type {1}", typeof(IdAttribute).Name, settings.GetType().Name));

			string id = (string)idField.GetValue(settings);
			if (id == null) throw new ApplicationException(String.Format("Detected null object ID"));

			return id;
        }
	}
}
