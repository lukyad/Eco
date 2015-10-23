﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.IO;
//using System.Reflection;
//using Eco.Extensions;
//using Eco.CodeBuilder;

//namespace Eco
//{
//    /// <summary>
//    /// Instructs serializer to load value for the given field from another file 
//    /// by using SettingsManager.Load method with the specified file format.
//    /// 
//    /// Usage: 
//    /// Can be applied to a field of a settings type only. Type of the 'extrnal' field must be marked with the Root attribute.
//    /// 
//    /// Comatibility: 
//    /// Compatible only with the Doc, Required and Optional attributes and incompatible with all other settings attributes.
//    /// </summary>
//    public class ExternalAttribute : FieldMutatorAttribute
//    {
//        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
//        {
//            typeof(ChoiceAttribute),
//            typeof(ConverterAttribute),
//            typeof(InlineAttribute),
//            typeof(RenameAttribute),
//            typeof(KnownTypesAttribute),
//            typeof(ParserAttribute),
//            typeof(PolymorphicAttribute),
//            typeof(RefAttribute)
//        };

//        public ExternalAttribute()
//            : base(GetRawSettingsFieldType, GetRawSettingsFieldAttributeText, GetRawSettingsFieldValue, SetRawSettingsFieldValueValue)
//        {
//        }

//        public override void ValidateContext(FieldInfo context)
//        {
//            if (!context.FieldType.IsSettingsType())
//                ThrowExpectedFieldOf("a settings type", context);

//            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
//        }

//        static new Type GetRawSettingsFieldType(FieldInfo refinedSettingsField)
//        {
//            return typeof(include<>).MakeGenericType(refinedSettingsField.FieldType);
//        }

//        static new string GetRawSettingsFieldAttributeText(FieldInfo refinedSettingsField)
//        {
//            return null;
//        }

//        static new object GetRawSettingsFieldValue(FieldInfo rawSettingsField, object rawSettings)
//        {
//            object includeElement = rawSettingsField.GetValue(rawSettings);
//            if (includeElement == null) return null;

//            include<object> includePrototype;
//            string fileName = (string)includeElement.GetFieldValue(nameof(includePrototype.file));
//            if (!File.Exists(fileName)) throw new ConfigurationException("Configuration file '{0}' doesn't exist", fileName);

//            Type externalSettingsType = (Type)includeElement.GetFieldValue(nameof(include<object>.refinedSettingsType));//rawSettingsField.GetCustomAttribute<ExternalSettingsTypeAttribute>().Type;
//            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
//            using (var reader = new StreamReader(fileStream))
//                return Settings.DefaultManager.Serializer.Deserialize(externalSettingsType, reader);
//        }

//        static void SetRawSettingsFieldValueValue(FieldInfo rawSettingsField, object rawSettings, object nonMutatedRawSettingsValue)
//        {
//            // We do not save included configuration files back to the drive. (ie included field are immutable).
//            // Thus, do nothing.
//        }
//    }
//}
