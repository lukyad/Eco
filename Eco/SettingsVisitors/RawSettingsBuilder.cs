﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// Used by the Eco library when writing settings to a stream.
    /// Given an object graph of refined settings initialize parallel graph of the corresponding raw settings.
    /// </summary>
    public class RawSettingsBuilder : ITwinSettingsVisitor
    {
        readonly Dictionary<Type, Type> _typeMappings = new Dictionary<Type, Type>();

        // Raw settings built by the RawSettingsBuilder can be converted back to 
        // the refined settings by the RefinedSettingsBuilder. Thus, it considered to be a revocable visitor.
        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRefinedSettingsType, Type rootRawSettingsType)
        {
            _typeMappings.Clear();
            _typeMappings.Add(rootRefinedSettingsType, rootRawSettingsType);
        }

        public void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings) { }

        public void Visit(string settingsNamesapce, string fieldPath, object refinedSettings, FieldInfo refinedSettingsField, object rawSettings, FieldInfo rawSettingsField)
        {
            if (refinedSettingsField.IsDefined<RefAttribute>()) return;

            object rawValue = null;
            object refinedValue = refinedSettingsField.GetValue(refinedSettings);
            if (refinedValue != null)
            {
                Type refinedValueType = refinedValue.GetType();
                if (refinedValueType.IsSettingsType())
                {
                    rawValue = SettingsConstruction.CreateSettingsObject(refinedValue, rawSettingsField, _typeMappings);
                }
                else if (refinedValueType.IsSettingsOrObjectArrayType())
                {
                    rawValue = SettingsConstruction.CreateSettingsArray((Array)refinedValue, rawSettingsField, _typeMappings);
                }
                else if (refinedValueType != typeof(string) && rawSettingsField.FieldType == typeof(string))
                {
                    rawValue = ToString(refinedSettingsField, refinedSettings);
                }
                else
                {
                    rawValue = refinedSettingsField.GetValue(refinedSettings);
                }
                rawSettingsField.SetValue(rawSettings, rawValue);
            }
        }

        public static string ToString(FieldInfo sourceField, object container)
        {
            object value = sourceField.GetValue(container);
            // Handle nullable types here
            if (value != null && Nullable.GetUnderlyingType(sourceField.FieldType) != null)
            {
                bool hasValue = (bool)sourceField.FieldType.GetProperty("HasValue").GetValue(value);
                if (hasValue) value = sourceField.FieldType.GetProperty("Value").GetValue(value);
            }

            ConverterAttribute converter = sourceField.GetCustomAttribute<ConverterAttribute>();
            if (converter != null)
                return converter.ToString(value, sourceField);
            else
                return value?.ToString();
        }
    }
}
