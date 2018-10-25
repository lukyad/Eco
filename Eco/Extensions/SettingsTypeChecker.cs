using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Eco.Extensions
{
    static class SettingsTypeChecker
    {
        static ConcurrentDictionary<string, byte> _knownSettingNamesapces = new ConcurrentDictionary<string, byte>();
        static ConcurrentDictionary<string, byte> _knownNonSettingNamesapces = new ConcurrentDictionary<string, byte>();

        public static bool IsSettingsType(Type t)
        {
            if (String.IsNullOrEmpty(t.Namespace)) return false;
            if (_knownNonSettingNamesapces.ContainsKey(t.Namespace)) return false;
            if (t.IsArray || !t.IsClass || t.IsDefined<NonSettingsTypeAttribute>() || t.IsDefined<CompilerGeneratedAttribute>()) return false;
            if (_knownSettingNamesapces.ContainsKey(t.Namespace)) return true;

            var settingsAssemblyAttr = t.Assembly.GetCustomAttribute<SettingsAssemblyAttribute>();
            bool isSettingsType =
                settingsAssemblyAttr != null && (
                String.IsNullOrEmpty(settingsAssemblyAttr.SettingsTypesNamesapace) ||
                t.Namespace.StartsWith(settingsAssemblyAttr.SettingsTypesNamesapace));

            if (isSettingsType)
            {
                _knownSettingNamesapces.TryAdd(t.Namespace, 0);
                return true;
            }
            else
            {
                _knownNonSettingNamesapces.TryAdd(t.Namespace, 0);
                return false;
            }
        }
    }
}
