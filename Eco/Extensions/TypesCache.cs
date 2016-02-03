using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Eco.Extensions
{
    static class TypesCache
    {
        static readonly ConcurrentDictionary<Assembly, Type[]> _assemblyTypes = new ConcurrentDictionary<Assembly, Type[]>();
        static readonly ConcurrentDictionary<Type, Type[]> _referencedSettingTypes = new ConcurrentDictionary<Type, Type[]>();
        static readonly ConcurrentDictionary<FieldInfo, Type[]> _knownSerializableTypes = new ConcurrentDictionary<FieldInfo, Type[]>();

        public static Type[] GetAssemblyTypes(Assembly source)
        {
            return _assemblyTypes.GetOrAdd(source, a => a.GetTypes());
        }

        public static Type[] GetReferencedSettingsTypes(Type root)
        {
            return _referencedSettingTypes.GetOrAdd(root, t => GetReferencedSettingsTypesRecursive(t, new HashSet<Type>()).ToArray());
        }

        public static Type[] GetKnownSerializableTypes(FieldInfo field)
        {
            return _knownSerializableTypes.GetOrAdd(field, f => GetKnownSerializableTypesFor(field).ToArray());
        }

        static IEnumerable<Type> GetReferencedSettingsTypesRecursive(Type rootType, HashSet<Type> visitedTypes)
        {
            if (rootType != null && rootType.IsSettingsType() && !visitedTypes.Contains(rootType))
            {
                yield return rootType;
                visitedTypes.Add(rootType);

                foreach (var type in GetReferencedTypesFor(rootType))
                {
                    Type settingsType = null;
                    if (type.IsSettingsType())
                        settingsType = type;
                    else if (type.IsSettingsArrayType())
                        settingsType = type.GetElementType();

                    foreach (var t in GetReferencedSettingsTypesRecursive(settingsType, visitedTypes))
                        yield return t;
                }

                //foreach (var derivedType in rootType.DerivedTypes())
                //{
                //    foreach (var t in GetReferencedSettingsTypesRecursive(derivedType, visitedTypes))
                //        yield return t;
                //}
            }
        }

        static IEnumerable<Type> GetReferencedTypesFor(Type type)
        {
            yield return type.BaseType;

            foreach (var field in type.GetOwnFields())
            {
                foreach (var t in field.GetKnownSerializableTypes())
                    yield return t;
            }
        }

        static IEnumerable<Type> GetKnownSerializableTypesFor(FieldInfo field)
        {
            var knownTypesAttributes = field.GetCustomAttributes<KnownTypesAttribute>().ToArray();
            IEnumerable<Type> knownTypes = Enumerable.Empty<Type>();
            if (knownTypesAttributes.Length > 0)
            {
                foreach (var a in knownTypesAttributes)
                    knownTypes = knownTypes.Concat(a.KnownTypes);
            }
            else
            {
                Type baseType;
                if (field.FieldType.IsArray) baseType = field.FieldType.GetElementType();
                else baseType = field.FieldType;

                knownTypes = baseType.GetDerivedTypes().Append(baseType);
            }

            var serializableKnownTypes = knownTypes.Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition);
            foreach (var t in serializableKnownTypes)
                yield return t;
        }
    }
}