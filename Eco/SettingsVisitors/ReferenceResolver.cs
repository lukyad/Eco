using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco
{
    public class ReferenceResolver : ITwinSettingsVisitor
    {
        readonly Dictionary<string, object> _settingsById;
        
        public ReferenceResolver(Dictionary<string, object> settingsById)
        {
            _settingsById = settingsById;
        }

        public const string NonExactMatchControlCharacter = "~";

        public const string FieldNameControlCharacter = "$";

        public bool IsReversable { get { return true; } }

        public void Initialize(Type rootRefinedSettingsType, Type rootRawSettingsType) { }

        public void Visit(string settingsNamespace, string settingsPath, object refinedSettings, object rawSettings) { }

        public void Visit(string settingsNamesapce, string fieldPath, FieldInfo refinedSettingsField, object refinedSettings, FieldInfo rawSettingsField, object rawSettings)
        {
            if (refinedSettingsField.IsDefined<RefAttribute>())
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
            foreach (string jointWildcard in wildcards.Split(','))
                settings.UnionWith(MatchJointWildcard(currentNamespace, fieldPath, jointWildcard, context));

            return settings.ToArray();
        }

        IEnumerable<object> MatchJointWildcard(string currentNamespace, string fieldPath, string jointWildcard, FieldInfo context)
        {
            var settings = new HashSet<object>();
            string[] joints = jointWildcard.Split('|');
            for (int i = 0; i < joints.Length; i++)
            {
                settings.UnionWith(MatchWildcard(currentNamespace, joints[i].Trim(), context));
                if (settings.Count > 0) break;
            }

            bool throwIfMissing = !context.GetCustomAttribute<RefAttribute>().IsWeak;
            if (settings.Count == 0 && throwIfMissing)
                throw new ConfigurationException("Could not find any settings matching the '{0}' id wildcard, referenced by {1}.", jointWildcard, fieldPath);

            return settings;
        }

        IEnumerable<object> MatchWildcard(string currentNamespace, string wildcard, FieldInfo context)
        {
            var settings = new HashSet<object>();
            var parts = wildcard.Split(':');
            bool hasTypeWildcard = parts.Length > 1;
            Wildcard idWildcard = IdWildcard(currentNamespace, parts[0], hasTypeWildcard, context);
            Wildcard typeWildcard = parts.Length > 1 ? TypeWildcard(parts[1], context) : null;
            var matchedIds = _settingsById.Keys.Where(id => idWildcard.IsMatch(id)).ToArray();
            for (int i = 0; i < matchedIds.Length; i++)
            {
                object candidate = _settingsById[matchedIds[i]];
                if (typeWildcard == null || IsOfType(typeWildcard, candidate))
                    settings.Add(candidate);
            }
            return settings;
        }

        static Wildcard IdWildcard(string currentNamespace, string pattern, bool hasTypeWildcard, FieldInfo context)
        {
            pattern = pattern.Trim();
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
            pattern = pattern.Replace(FieldNameControlCharacter, context.Name);
            // ~ at the start of the string means that non-exact type match is allowed
            if (pattern.StartsWith(NonExactMatchControlCharacter))
                pattern = Wildcard.Everything + pattern.Trim(NonExactMatchControlCharacter[0]) + Wildcard.Everything;

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
            return _settingsById.First(p => p.Value == settings).Key;
        }

        static string FieldDescription(FieldInfo field)
        {
            return field.DeclaringType.Name + SettingsPath.Separator + field.Name;
        }
    }
}
