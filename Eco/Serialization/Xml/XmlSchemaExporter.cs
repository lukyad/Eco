using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Reflection;
using Eco.Extensions;

namespace Eco.Serialization.Xml
{
    public static class XmlSchemaExporter
    {
        const decimal MaxOccursUnbounded = 79228162514264337593543950335M;

        public static string GetSchemaFor<T>(Usage defaultUsage)
        {
            var xmlSettingsType = SerializableTypeEmitter.GetSchemaTypeFor<T>(new XmlAttributesGenerator(), defaultUsage);
            var importer = new XmlReflectionImporter();
            var schemas = new XmlSchemas();
            var exporter = new System.Xml.Serialization.XmlSchemaExporter(schemas);
            var map = importer.ImportTypeMapping(xmlSettingsType);
            exporter.ExportTypeMapping(map);

            var schema = schemas[0];
            ApplyCustomAttributes(xmlSettingsType, schema);
            Replase_Sequence_With_All_ForNonCollectionTypes(schema);
            SetSchemaNamespaces(xmlSettingsType, schema);

            using (var ms = new MemoryStream())
            {
                schemas[0].Write(ms);
                ms.Position = 0;
                return new StreamReader(ms).ReadToEnd();
            }
        }

        #region Apply Annotation and Usage attribues

        static void ApplyCustomAttributes(Type type, XmlSchema schema)
        {
            var customAttributes = BuildCustomAttributesMap(type);
            foreach (var item in schema.Items)
            {
                var complexType = item as XmlSchemaComplexType;
                if (complexType != null)
                    ApplyCustomAttributes(complexType, customAttributes);
            }
        }

        static void ApplyCustomAttributes(XmlSchemaComplexType type, Dictionary<string, SchemaNodeAttributes> customAttributes)
        {
            SchemaNodeAttributes attributes;
            if (customAttributes.TryGetValue(type.Name, out attributes))
                type.Annotation = TextToAnnotation(attributes.Annotation);

            var subGroup = type.Particle as XmlSchemaGroupBase;
            if (subGroup != null)
                ApplyCustomAttributes(type.Name, subGroup.Items, customAttributes);

            var complexContent = type.ContentModel as XmlSchemaComplexContent;
            if (complexContent != null)
            {
                var extension = complexContent.Content as XmlSchemaComplexContentExtension;
                if (extension != null)
                    ApplyCustomAttributes(type.Name, extension, customAttributes);
            }

            ApplyCustomAttributes(type.Name, type.Attributes, customAttributes);
        }

        static void ApplyCustomAttributes(string typeName, XmlSchemaObjectCollection items, Dictionary<string, SchemaNodeAttributes> customAttributes)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    var element = item as XmlSchemaElement;
                    if (element != null)
                        ApplyCustomAttributes(typeName, element, customAttributes);

                    var attribute = item as XmlSchemaAttribute;
                    if (attribute != null)
                        ApplyCustomAttributes(typeName, attribute, customAttributes);
                }
            }
        }

        static void ApplyCustomAttributes(string typeName, XmlSchemaElement schema, Dictionary<string, SchemaNodeAttributes> customAttributes)
        {
            string nodeKey = GetFieldKey(typeName, schema.Name);
            SchemaNodeAttributes nodeAttributes;
            if (customAttributes.TryGetValue(nodeKey, out nodeAttributes))
            {
                schema.Annotation = TextToAnnotation(nodeAttributes.Annotation);
                ApplyUsageAttribute(schema, nodeAttributes.Required);
            }

        }

        static void ApplyCustomAttributes(string typeName, XmlSchemaAttribute schema, Dictionary<string, SchemaNodeAttributes> customAttributes)
        {
            string nodeKey = GetFieldKey(typeName, schema.Name);
            SchemaNodeAttributes nodeAttributes;
            if (customAttributes.TryGetValue(nodeKey, out nodeAttributes))
            {
                schema.Annotation = TextToAnnotation(nodeAttributes.Annotation);
                ApplyUsageAttribute(schema, nodeAttributes.Required);
            }
        }

        static void ApplyCustomAttributes(string typeName, XmlSchemaComplexContentExtension extension, Dictionary<string, SchemaNodeAttributes> customAttributes)
        {
            var subGroup = extension.Particle as XmlSchemaGroupBase;
            if (subGroup != null)
                ApplyCustomAttributes(typeName, subGroup.Items, customAttributes);

            ApplyCustomAttributes(typeName, extension.Attributes, customAttributes);
        }

        #region Convert text to Annotation

        static XmlSchemaAnnotation TextToAnnotation(string text)
        {
            if (text != null)
            {
                var documentation = new XmlSchemaDocumentation();
                documentation.Markup = TextToNodeArray(text);

                var annotation = new XmlSchemaAnnotation();
                annotation.Items.Add(documentation);

                return annotation;
            }

            return null;
        }

        static XmlNode[] TextToNodeArray(string text)
        {
            XmlDocument doc = new XmlDocument();
            return new XmlNode[1] { doc.CreateTextNode(text) };
        }

        #endregion

        #region Apply Usage attribute

        static void ApplyUsageAttribute(XmlSchemaAttribute schema, bool required)
        {
            if (required)
                schema.Use = XmlSchemaUse.Required;
            else
                schema.Use = XmlSchemaUse.Optional;
        }

        static void ApplyUsageAttribute(XmlSchemaElement schema, bool required)
        {
            if (required)
                schema.MinOccurs = 1;
            else
                schema.MinOccurs = 0;
        }

        #endregion

        #region Build Attributes Map

        static Dictionary<string, SchemaNodeAttributes> BuildCustomAttributesMap(Type type)
        {
            var map = new Dictionary<string, SchemaNodeAttributes>();

            foreach (var t in type.GetReferencedSettingsTypesRecursive())
            {
                AddTypeAttributes(t, map);
                var publicFields = t.GetOwnFields();
                foreach (var field in publicFields)
                    AddFieldAttributes(field, map);
            }
            return map;
        }

        static void AddTypeAttributes(Type type, Dictionary<string, SchemaNodeAttributes> map)
        {
            map.Add(type.Name, GetNodeAttributes(type));
        }

        static void AddFieldAttributes(FieldInfo field, Dictionary<string, SchemaNodeAttributes> map)
        {
            var key = GetFieldKey(field.ReflectedType.Name, field.GetOverridenName());
            map.Add(key, GetNodeAttributes(field));
        }

        static string GetFieldKey(string typeName, string fieldName)
        {
            return String.Format("{0}.{1}", typeName, fieldName);
        }

        static SchemaNodeAttributes GetNodeAttributes(MemberInfo attrProvider)
        {
            var documentationAttr = attrProvider.GetCustomAttribute<DocAttribute>(false);
            return new SchemaNodeAttributes
            {
                Annotation = documentationAttr != null ? documentationAttr.Annotation : null,
                Required = attrProvider.IsDefined<RequiredAttribute>(false) || !attrProvider.IsDefined<OptionalAttribute>(false)
            };
        }

        #endregion

        #endregion

        #region Replace SchemaSequence with SchemaAll for non-collection types

        static void Replase_Sequence_With_All_ForNonCollectionTypes(XmlSchema schema)
        {
            foreach (var item in schema.Items)
            {
                var complexType = item as XmlSchemaComplexType;
                if (complexType != null)
                {
                    bool isClassType = !IsCollectionType(complexType);
                    var sequence = complexType.Particle as XmlSchemaSequence;
                    if (isClassType && sequence != null && !HasChoiseChildElement(sequence))
                        complexType.Particle = CreateAllFromSequence(sequence);
                }
            }
        }

        static bool HasChoiseChildElement(XmlSchemaSequence sequence)
        {
            foreach (var item in sequence.Items)
                if (item is XmlSchemaChoice)
                    return true;

            return false;
        }

        static bool IsCollectionType(XmlSchemaComplexType type)
        {
            var sequence = type.Particle as XmlSchemaSequence;
            if (sequence != null)
            {
                foreach (XmlSchemaParticle element in sequence.Items)
                    if (element.MaxOccurs == MaxOccursUnbounded)
                        return true;
            }

            return false;
        }

        static XmlSchemaAll CreateAllFromSequence(XmlSchemaSequence sequence)
        {
            var all = new XmlSchemaAll();
            foreach (var item in sequence.Items)
                all.Items.Add(item);

            return all;
        }

        #endregion

        #region Set Schema namespaces

        static void SetSchemaNamespaces(Type type, XmlSchema schema)
        {
            var namespc = String.Format("{0}.xsd", type.Name);
            schema.TargetNamespace = namespc;
            schema.Namespaces.Add(String.Empty, namespc);
        }

        #endregion

        class SchemaNodeAttributes
        {
            public string Annotation;

            public bool Required;
        }
    }
}
