﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.FieldVisitors
{
	class SettingsMapBuilder : IFieldVisitor
	{
		readonly Dictionary<string, object> _settingsById = new Dictionary<string, object>();

		public Dictionary<string, object> SettingsByIdMap { get { return _settingsById; } }

		public void Visit(FieldInfo rawSettingsField, object rawSettings, FieldInfo refinedSettingsField, object refinedSettings)
		{
			if (rawSettingsField.IsDefined<IdAttribute>())
			{
				string id = (string)rawSettingsField.GetValue(rawSettings);
				if (_settingsById.ContainsKey(id)) throw new ConfigurationException("Duplicated settings ID: '{0}'", id);
				_settingsById.Add(id, refinedSettings);
			}
		}
	}
}
