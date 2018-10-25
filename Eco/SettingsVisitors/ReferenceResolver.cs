using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Eco.Extensions;

namespace Eco.SettingsVisitors
{
    // Resolves settings references.
    //
    // Could generate and extra settings that were not present in the raw settings graph to the refinedSettingsById map.
    // This happens when reference is initialized with the prototype syntax, e.g. .*:myType { myValue = 0.1 }
    // In the example above Eco finds the prototype settins object matched by the .*:myType
    // and then creates a new instance of the myType object by making a shallow copy of the prototype
    // and initializing the new instance fields as specified in the `{ }` inintialization list.
    // The new instance is then added to the refinedSettingsById map with a normalized id.
    // Normalized id means that two NOT EQUAL `generating` references that result in instantiation of two EQUAL objects
    // would have the same id and would resolve to a single settings object (i.e. the first resolved reference would generate an extra object
    // and the second one would be resolved to the already generated object).
    public class ReferenceResolver : ITwinSettingsVisitor
    {
        readonly Dictionary<string, object> _refinedSettingsById;
        readonly Dictionary<object, object> _refinedToRawMap;
        // ReferenceResolver adds dynamicly generated settings to the global twinSettingsList for the futher processing by the following visitors.
        readonly TwinSettingsListBuilder _twinSettingsListBuilder;
        readonly HashSet<object> _prototypes = new HashSet<object>();
        readonly HashSet<Type> _settingTypesFilter;
        Dictionary<object, string> _idByRefinedSettings;
        ParsingPolicyAttribute[] _parsingPolicies;

        public ReferenceResolver(Dictionary<string, object> refinedSettingsById, Dictionary<object, object> refinedToRawMap, TwinSettingsListBuilder twinSettingsListBuilder, params Type[] settingTypesFilter)
        {
            _refinedSettingsById = refinedSettingsById;
            _refinedToRawMap = refinedToRawMap;
            _twinSettingsListBuilder = twinSettingsListBuilder;
            _settingTypesFilter = settingTypesFilter != null && settingTypesFilter.Length > 0 ? new HashSet<Type>(settingTypesFilter) : null;
        }
        public static class ControlChars
        {
            public const string NonExactMatch = "~";

            public const string FieldNameAlias = "$";

            public const char WildcardJoiner = '|';

            public const char WildcardTypeSeparator = ':';

            public const char WildcardParamsSeparator = ',';

            public const char WildcardParamValueSeparator = '=';
        }


        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRefinedSettingsType, Type rootRawSettingsType)
        {
            // Capture parsing policies that applies to all fields.
            _parsingPolicies = ParsingPolicyAttribute.GetPolicies(rootRefinedSettingsType);
            _idByRefinedSettings = _refinedSettingsById.ToDictionary(p => p.Value, p => p.Key);
        }

        public void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings) { }

        public void Visit(string settingsNamesapce, string fieldPath, object refinedSettings, FieldInfo refinedSettingsField, object rawSettings, FieldInfo rawSettingsField)
        {
            var refinedSettingsType = refinedSettings.GetType();
            bool passTypeFilter =
                _settingTypesFilter == null ||
                _settingTypesFilter.Contains(refinedSettingsType) ||
                refinedSettingsType.IsGenericType && _settingTypesFilter.Contains(refinedSettingsType.GetGenericTypeDefinition());

            if (passTypeFilter && refinedSettingsField.IsDefined<RefAttribute>())
            {
                if (refinedSettingsField.FieldType.IsArray) ResolveReferenceArray(settingsNamesapce, fieldPath, rawSettingsField, rawSettings, refinedSettingsField, refinedSettings);
                else ResolveReference(settingsNamesapce, fieldPath, rawSettingsField, rawSettings, refinedSettingsField, refinedSettings);
            }
        }

        void ResolveReference(string settingsNamesapce, string fieldPath, FieldInfo rawSettingsField, object rawSettings, FieldInfo refinedSettingsField, object refinedSettings)
        {
            string idWildcard = (string)rawSettingsField.GetValue(rawSettings);
            if (idWildcard != null)
            {
                object[] matches = MatchSettings(settingsNamesapce, fieldPath, idWildcard, refinedSettingsField);
                if (matches.Length > 1)
                {
                    bool isWeakRef = refinedSettingsField.GetCustomAttribute<RefAttribute>().IsWeak;
                    throw new ConfigurationException("Wildcard '{0}' used by '{1}' matches more than one settings object, when expected {2}",
                        idWildcard,
                        fieldPath,
                        isWeakRef ? "exactly one or no matches." : "exactly one match.");
                }
                object settings = matches.Where(s => s != Settings.Null).SingleOrDefault();
                if (settings != null && !refinedSettingsField.FieldType.IsAssignableFrom(settings.GetType()))
                {
                    throw new ConfigurationException("Could not assign object with ID='{0}' of type '{1}' to the '{2}' field of type '{3}'.",
                        IdOf(settings), settings.GetType().Name, fieldPath, refinedSettingsField.FieldType.Name);
                }
                refinedSettingsField.SetValue(refinedSettings, settings);
            }
        }

        void ResolveReferenceArray(string settingsNamesapce, string fieldPath, FieldInfo rawSettingsField, object rawSettings, FieldInfo refinedSettingsField, object refinedSettings)
        {
            string idWildcards = (string)rawSettingsField.GetValue(rawSettings);
            if (idWildcards != null)
            {
                object[] matches =
                    MatchSettings(settingsNamesapce, fieldPath, idWildcards, refinedSettingsField)
                    .Where(s => s != Settings.Null)
                    .ToArray();
                if (matches.Length > 0)
                {
                    Type elementType = refinedSettingsField.FieldType.GetElementType();
                    Array settingsArray = Array.CreateInstance(elementType, matches.Length);
                    for (int i = 0; i < matches.Length; i++)
                    {
                        object settings = matches[i];
                        if (!elementType.IsAssignableFrom(settings.GetType()))
                        {
                            throw new ConfigurationException("Could not assign object with ID='{0}' of type '{1}' to an element of the array '{2}' of type '{3}'.",
                                IdOf(settings), settings.GetType().Name, fieldPath, elementType.Name);
                        }
                        settingsArray.SetValue(matches[i], i);
                    }
                    refinedSettingsField.SetValue(refinedSettings, settingsArray);
                }
            }
        }

        object[] MatchSettings(string currentNamespace, string fieldPath, string wildcards, FieldInfo context)
        {
            var elementType = context.FieldType.GetElementType();
            var settings = new HashSet<object>();
            foreach (string jointWildcard in wildcards.Split(Settings.IdSeparator))
                settings.UnionWith(MatchJointReference(currentNamespace, fieldPath, jointWildcard, context));

            return settings.ToArray();
        }

        IEnumerable<object> MatchJointReference(string currentNamespace, string fieldPath, string jointWildcard, FieldInfo context)
        {
            var settings = new HashSet<object>();
            string[] joints = jointWildcard.Split(ControlChars.WildcardJoiner);
            for (int i = 0; i < joints.Length; i++)
            {
                settings.UnionWith(MatchReference(currentNamespace, fieldPath, joints[i].Trim(), context));
                if (settings.Count > 0) break;
            }

            bool throwIfMissing = !context.GetCustomAttribute<RefAttribute>().IsWeak;
            if (settings.Count == 0 && throwIfMissing)
                throw new ConfigurationException("Could not find any settings matching the '{0}' id wildcard, referenced by {1}.", jointWildcard, fieldPath);

            return settings;
        }

        IEnumerable<object> MatchReference(string currentNamespace, string currentPath, string wildcard, FieldInfo context)
        {
            var settings = new HashSet<object>();

            var match = Regex.Match(wildcard, @"(?<id>[^\:]*)?(?:\s*\" + ControlChars.WildcardTypeSeparator + @"\s*(?<type>[^\{]+))?(?:\s*\{(?<params>.*)\})?");
            if (!match.Success)
                throw new ConfigurationException("Invalid reference: {0}.", wildcard);

            bool hasTypeWildcard = !String.IsNullOrWhiteSpace(match.Groups["type"].Value);
            Wildcard idWildcard = IdWildcard(currentNamespace, match.Groups["id"].Value, hasTypeWildcard, context);
            Wildcard typeWildcard = hasTypeWildcard ? TypeWildcard(match.Groups["type"].Value, context) : null;
            var matchedIds = _refinedSettingsById.Keys.Where(id => idWildcard.IsMatch(id)).ToArray();
            for (int i = 0; i < matchedIds.Length; i++)
            {
                object candidate = _refinedSettingsById[matchedIds[i]];
                if (typeWildcard == null || IsOfType(typeWildcard, candidate))
                    settings.Add(candidate);
            }

            // If params are specified, then the result object is instantiated from the specified prototype
            // and initialied with the specified field values.
            if (!String.IsNullOrWhiteSpace(match.Groups["params"].Value))
            {
                if (settings.Count == 0)
                    throw new ConfigurationException("The {0} wildcard doesn't matche any settings.", wildcard);

                object proto = settings.FirstOrDefault(s => _prototypes.Contains(s));
                if (proto == null)
                {
                    if (settings.Count == 1)
                    {
                        proto = settings.Single();
                        _prototypes.Add(proto);
                    }
                    else
                        throw new ConfigurationException("The {0} wildcard matches more than one prototype.", wildcard);
                }

                (object dynamicRefinedSettings, object dynamicRawSettings, string normalizedParams) = 
                    CreateInstanse(proto, _refinedToRawMap[proto], match.Groups["params"].Value, _parsingPolicies);

                string protoId = _idByRefinedSettings[proto];
                string dynamicSettingsId = $"{protoId} {{ {normalizedParams} }}";

                if (!_refinedSettingsById.TryGetValue(dynamicSettingsId, out object existingDynamicSettings))
                {
                    // Make SettingsManager aware of the dynamicly generated settings.
                    _refinedSettingsById.Add(dynamicSettingsId, dynamicRefinedSettings);
                    _refinedToRawMap.Add(dynamicSettingsId, dynamicRawSettings);

                    SettingsManager.TraverseTwinSeetingsTrees(
                        startNamespace: currentNamespace,
                        startPath: currentPath,
                        rootMasterSettings: dynamicRefinedSettings,
                        rootSlaveSettings: dynamicRawSettings,
                        visitor: _twinSettingsListBuilder,
                        initVisitor: false);
                }
                else
                    dynamicRefinedSettings = existingDynamicSettings;

                settings.Clear();
                settings.Add(dynamicRefinedSettings);
            }

            return settings;
        }

        static Wildcard IdWildcard(string currentNamespace, string pattern, bool hasTypeWildcard, FieldInfo context)
        {
            pattern = pattern?.Trim();
            // Matches null settings.
            if (pattern == Settings.NullId) return new Wildcard(Settings.NullId);
            // If id wildcard is not specified, then set it to * to match everything in the current namesapce.
            if (String.IsNullOrEmpty(pattern)) pattern = Wildcard.Everything;
            // If we got compund settings wildcard (ie id + type), and the id part of the wildcard doesn't end with *,
            // then we assume that the id part specifies namespace and the type part specifies a single object from that namespace.
            // To match all settings from the provided namespace we add * to the id wildcard.
            // We filter by the settings type later on.
            if (hasTypeWildcard && !pattern.Contains(Wildcard.Everything)) pattern += Wildcard.Everything;
            return
                pattern.StartsWith(SettingsPath.Separator.ToString()) ?
                // If wildcard starts with the dot char, then it refers to id from the global namespace (not local). 
                // Thus we do not apply local namespace in this case
                new Wildcard(pattern.Trim(' ', SettingsPath.Separator)) :
                // Apply local namespace to the wildcard.
                new Wildcard(SettingsPath.Combine(currentNamespace, pattern.Trim()));
        }

        static Wildcard TypeWildcard(string pattern, FieldInfo context)
        {
            pattern = pattern.Trim();
            pattern = pattern.Replace(ControlChars.FieldNameAlias, context.Name);
            // ~ at the start of the string means that non-exact type match is allowed
            if (pattern.StartsWith(ControlChars.NonExactMatch))
                pattern = Wildcard.Everything + pattern.Trim(ControlChars.NonExactMatch[0]) + Wildcard.Everything;

            return new Wildcard(pattern.ToLower());
        }

        static bool IsOfType(Wildcard typeWildcard, object o)
        {
            return GetInheritedSettingTypes(o.GetType()).Any(t => typeWildcard.IsMatch(t.Name.ToLower()));
        }

        static IEnumerable<Type> GetInheritedSettingTypes(Type type)
        {
            if (type.IsSettingsType())
                yield return type;
            else
                yield break;
            foreach (var baseType in GetInheritedSettingTypes(type.BaseType))
                yield return baseType;
        }

        string IdOf(object settings)
        {
            return _refinedSettingsById.First(p => p.Value == settings).Key;
        }

        static (object refinedSettins, object rawProto, string normilizedParams) CreateInstanse(object refinedProto, object rawProto, string parameters, ParsingPolicyAttribute[] parsingPolicies)
        {
            var refinedResult = Cloner.Clone(refinedProto);
            var rawResult = Cloner.Clone(rawProto);
            var props = parameters.SplitAndTrim(ControlChars.WildcardParamsSeparator);
            string normilizedParams = null;
            foreach (var p in props)
            {
                var parts = p.SplitAndTrim(ControlChars.WildcardParamValueSeparator);
                if (parts.Length != 2)
                    Throw();

                rawResult.SetFieldValue(parts[0], parts[1]);
                object fieldValue = ParseFieldValue(refinedResult, fieldName: parts[0], fieldValue: parts[1]);
                refinedResult.SetFieldValue(parts[0], fieldValue);

                FieldInfo field = refinedResult.GetType().GetField(parts[0]);
                string serializedValue = RawSettingsBuilder.ToString(sourceField: field, container: refinedResult);
                normilizedParams += $"{parts[0]}={serializedValue}{ControlChars.WildcardParamsSeparator}";
            }
            return (refinedResult, rawResult, normilizedParams.Trim(ControlChars.WildcardParamsSeparator));

            object ParseFieldValue(object container, string fieldName, string fieldValue)
            {
                var targetField = container.GetType().GetField(fieldName);
                if (targetField == null)
                    Throw();

                if (targetField.FieldType == typeof(string))
                    return fieldValue;

                return RefinedSettingsBuilder.FromString(fieldValue, targetField, parsingPolicies);
            }

            void Throw() => throw new ConfigurationException($"Invalid {refinedProto.GetType()} parameters definition: {parameters}");
        }

        static string FieldDescription(FieldInfo field)
        {
            return field.DeclaringType.Name + SettingsPath.Separator + field.Name;
        }
    }
}
