using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using Eco.Extensions;

namespace Eco.Serialization
{
    static class SerializableTypeEmitter
    {
        static Dictionary<Type, Type> _serializableTypeCache = new Dictionary<Type, Type>();
        static Dictionary<Type, Type> _schemaTypeCache = new Dictionary<Type, Type>();

        delegate Type GetRawFieldTypeDelegate(FieldInfo field, ConversionPolicyAttribute[] conversionPolicies);

        public static Type GetRawTypeFor<T>(ISerializationAttributesGenerator attributesGenerator, Usage defaultUsage)
        {
            return GetRawTypeFor(typeof(T), attributesGenerator, defaultUsage);
        }

        public static Type GetRawTypeFor(Type settingsType, ISerializationAttributesGenerator attributesGenerator, Usage defaultUsage)
        {
            if (settingsType == null) throw new ArgumentNullException(nameof(settingsType));
            lock (_serializableTypeCache)
            {
                Type serializableType;
                if (!_serializableTypeCache.TryGetValue(settingsType, out serializableType))
                {
                    serializableType = Emit(settingsType, attributesGenerator, GetRawFieldType, defaultUsage);
                    _serializableTypeCache.Add(settingsType, serializableType);
                }
                return serializableType;
            }
        }

        public static Type GetSchemaTypeFor<T>(ISerializationAttributesGenerator attributesGenerator, Usage defaultUsage)
        {
            return GetSchemaTypeFor(typeof(T), attributesGenerator, defaultUsage);
        }

        public static Type GetSchemaTypeFor(Type settingsType, ISerializationAttributesGenerator attributesGenerator, Usage defaultUsage)
        {
            if (settingsType == null) throw new ArgumentNullException(nameof(settingsType));
            lock (_serializableTypeCache)
            {
                Type schemaType;
                if (!_schemaTypeCache.TryGetValue(settingsType, out schemaType))
                {
                    schemaType = Emit(settingsType, attributesGenerator, GetSchemaFieldType, defaultUsage);
                    _schemaTypeCache.Add(settingsType, schemaType);
                }
                return schemaType;
            }
        }

        public static Type GetRawFieldType(this FieldInfo field, ConversionPolicyAttribute[] conversionPolicies)
        {
            var sourceType = field.FieldType;
            if (field.IsDefined(typeof(RefAttribute)) ||
                field.IsDefined(typeof(ConverterAttribute)) ||
                conversionPolicies.Any(p => p.SourceType == sourceType) ||
                Nullable.GetUnderlyingType(sourceType).IsSimple())
            {
                return typeof(string);
            }
            else if (field.IsDefined<FieldMutatorAttribute>())
            {
                return field.GetCustomAttribute<FieldMutatorAttribute>().GetRawSettingsFieldType(field);
            }
            else
            {
                return sourceType;
            }
        }

        public static Type GetSchemaFieldType(this FieldInfo field, ConversionPolicyAttribute[] conversionPolicies)
        {
            var sourceType = field.FieldType;
            if (field.IsDefined(typeof(RefAttribute)) || 
                field.IsDefined(typeof(ConverterAttribute)) ||
                conversionPolicies.Any(p => p.SourceType == sourceType))
            {
                return typeof(string);
            }
            else if (Nullable.GetUnderlyingType(sourceType).IsSimple())
            {
                return Nullable.GetUnderlyingType(sourceType);
            }
            else if (field.IsDefined<FieldMutatorAttribute>())
            {
                return field.GetCustomAttribute<FieldMutatorAttribute>().GetRawSettingsFieldType(field);
            }
            else
            {
                return sourceType;
            }
        }

        static Type Emit(
            Type rootSettingsType, 
            ISerializationAttributesGenerator attributesGenerator,
            GetRawFieldTypeDelegate GetRawFieldType, 
            Usage defaultUsage)
        {
            var conversionPolicies = rootSettingsType.GetCustomAttributes<ConversionPolicyAttribute>().ToArray(); ;
            string compilationUnit = GenerateClassDefinitionRecursive(rootSettingsType, attributesGenerator, GetRawFieldType, defaultUsage, conversionPolicies);
            string[] referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location)
                .ToArray();
            CompilerParameters p = new CompilerParameters(referencedAssemblies);
            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(p, compilationUnit);
            if (results.Errors.Count > 0) throw new ConfigurationException("Could not emit xml serialization classes for the '{0}' settings type", rootSettingsType.Name);
            return results.CompiledAssembly.GetTypes().First(t => t.Name == rootSettingsType.Name);
        }

        static string GenerateClassDefinitionRecursive(
            Type settingsType, 
            ISerializationAttributesGenerator attributesGenerator,
            GetRawFieldTypeDelegate GetRawFieldType,
            Usage defaultUsage,
            ConversionPolicyAttribute[] conversionPolicies)
        {
            var autogenNamespace = settingsType.Namespace + ".AutoGenerated";
            var codeBuilder = new CompilationUnitBuilder(autogenNamespace);
            codeBuilder.AddAssemblyAttribute(typeof(SettingsAssemblyAttribute).FullName);
            foreach (var type in settingsType.GetReferencedSettingsTypesRecursive())
            {
                string baseTypeName = type.BaseType.IsSettingsType() ? type.BaseType.GetNonGenericName() : null;
                var classBuilder = codeBuilder.AddClass(type.GetNonGenericName(), baseTypeName);
                classBuilder.AddAttributes(attributesGenerator.GetAttributesTextFor(type));
                foreach (var field in type.GetFields())
                {
                    ValidateFieldAttributes(field);
                    var serializableFieldType = GetRawFieldType(field, conversionPolicies);
                    var fieldBuilder = classBuilder.AddField(GetUnivocalTypeName(serializableFieldType), field.Name);
                    fieldBuilder.AddAttributes(attributesGenerator.GetAttributesTextFor(field, defaultUsage, conversionPolicies));
                }
            }
            return codeBuilder.ToString();
        }

        static string GetUnivocalTypeName(Type type)
        {
            if (type.IsSettingsType() || type.IsSettingsArrayType()) return type.GetNonGenericName();
            else return type.FullName;
        }

        static void ValidateFieldAttributes(FieldInfo field)
        {
            var ecoAttributes = field.GetCustomAttributes().OfType<EcoFieldAttribute>();
            foreach (var a in ecoAttributes)
                a.ValidateContext(field);
        }
    }
}
