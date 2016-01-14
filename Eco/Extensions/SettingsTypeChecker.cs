﻿using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Eco.Extensions
{
    static class SettingsTypeChecker
    {
        static ConcurrentDictionary<string, byte> _knownSettingNamesapces = new ConcurrentDictionary<string, byte>();
        static ConcurrentDictionary<string, byte> _knownNonSettingNamesapces = new ConcurrentDictionary<string, byte>();

        public static bool IsSettingsType(Type t)
        {
            if (String.IsNullOrEmpty(t.Namespace) || t.IsArray || !t.IsClass) return false;
            if (_knownSettingNamesapces.ContainsKey(t.Namespace)) return true;
            if (_knownNonSettingNamesapces.ContainsKey(t.Namespace)) return false;
            
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