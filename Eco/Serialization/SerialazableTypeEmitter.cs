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

        delegate Type GetRawFieldTypeDelegate(FieldInfo field, ParsingPolicyAttribute[] parsingPolicies);

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

        public static Type GetRawFieldType(this FieldInfo field, ParsingPolicyAttribute[] parsingPolicies)
        {
            var sourceType = field.FieldType;
            if (field.IsDefined(typeof(RefAttribute)) ||
                field.IsDefined(typeof(ConverterAttribute)) ||
                parsingPolicies.Any(p => p.CanParse(sourceType)) ||
                sourceType.IsNullable())
            {
                return typeof(string);
            }
            else
            {
                return sourceType;
            }
        }

        public static Type GetSchemaFieldType(this FieldInfo field, ParsingPolicyAttribute[] parsingPolicies)
        {
            var sourceType = field.FieldType;
            if (field.IsDefined(typeof(RefAttribute)) || 
                field.IsDefined(typeof(ConverterAttribute)) ||
                parsingPolicies.Any(p => p.CanParse(sourceType)))
            {
                return typeof(string);
            }
            else if (sourceType.IsNullable())
            {
                if (Nullable.GetUnderlyingType(sourceType).IsSimple())
                    return Nullable.GetUnderlyingType(sourceType);
                else
                    return typeof(string);
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
            var parsingPolicies = rootSettingsType.GetCustomAttributes<ParsingPolicyAttribute>().ToArray(); ;
            string compilationUnit = GenerateClassDefinitionRecursive(rootSettingsType, attributesGenerator, GetRawFieldType, defaultUsage, parsingPolicies);
            string[] referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location)
                .ToArray();
            CompilerParameters p = new CompilerParameters(referencedAssemblies);
            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(p, compilationUnit);
            if (results.Errors.Count > 0)
            {
                throw new ConfigurationException(
                    "Could not emit xml serialization classes for the '{0}' settings type: {1}{2}", 
                    rootSettingsType.Name, 
                    Environment.NewLine,
                    GetCompilationErrorsDescription(results.Errors));
            }
            return results.CompiledAssembly.GetTypes().First(t => t.Name == rootSettingsType.Name);
        }

        static string GetCompilationErrorsDescription(CompilerErrorCollection errors)
        {
            var description = new StringBuilder();
            foreach (var error in errors)
                description.AppendLine(error.ToString());
            return description.ToString();
        }

        static string GenerateClassDefinitionRecursive(
            Type settingsType, 
            ISerializationAttributesGenerator attributesGenerator,
            GetRawFieldTypeDelegate GetRawFieldType,
            Usage defaultUsage,
            ParsingPolicyAttribute[] parsingPolicies)
        {
            var autogenNamespace = settingsType.Namespace + ".AutoGenerated";
            var codeBuilder = new CompilationUnitBuilder(autogenNamespace);
            codeBuilder.AddAssemblyAttribute(typeof(SettingsAssemblyAttribute).FullName);
            foreach (var type in settingsType.GetReferencedSettingsTypesRecursive())
            {
                string baseTypeName = type.BaseType.IsSettingsType() ? type.BaseType.NonGenericName() : null;
                var classBuilder = codeBuilder.AddClass(type.NonGenericName(), baseTypeName);
                classBuilder.AddAttributes(attributesGenerator.GetAttributesTextFor(type));
                foreach (var field in type.OwnFields())
                {
                    ValidateFieldAttributes(field);
                    var serializableFieldType = GetRawFieldType(field, parsingPolicies);
                    var fieldBuilder = classBuilder.AddField(GetUnivocalTypeName(serializableFieldType), field.Name);
                    fieldBuilder.AddAttributes(attributesGenerator.GetAttributesTextFor(field, defaultUsage, parsingPolicies));
                }
            }
            return codeBuilder.ToString();
        }

        static string GetUnivocalTypeName(Type type)
        {
            if (type.IsSettingsType() || type.IsSettingsArrayType()) return type.NonGenericName();
            else return type.FullName;
        }

        static void ValidateFieldAttributes(FieldInfo field)
        {
            var ecoAttributes = field.GetCustomAttributes().OfType<EcoFieldAttribute>();
            foreach (var a in ecoAttributes)
                a.ValidateContext(field);

            var requiredAttrTypes = field.FieldType.GetCustomAttribute<RequiredAttributesAttribute>(inherit: false)?.AttributeTypes;
            if (requiredAttrTypes != null)
            {
                var fieldAttributes = field.GetCustomAttributes();
                foreach (var requiredAttrType in requiredAttrTypes)
                {
                    if (!fieldAttributes.Any(a => a.GetType() == requiredAttrType))
                    {
                        throw new ConfigurationException(
                            "Field of type '{0}' requires an attribute(s) of type '{1}'.", 
                            field.FieldType, 
                            requiredAttrTypes.Select(a => a.FullName).CommaWhiteSpaceSeparated());
                    }
                }
            }
            
        }
    }
}
