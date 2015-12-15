using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Eco.SettingsVisitors
{
    /// <summary>
    /// This one is used internally by the Eco library to apply defaults to the settings that pass the id filter.
    /// </summary>
    class DefaultsSetter : ITwinSettingsVisitor
    {
        public bool IsReversable { get { return false; } }

        public void Initialize(Type rootDefaultsType, Type rootTargetType) { }
        

        public void Visit(string settingsNamespace, string settingsPath, object defaults, object target)
        {
        }

        public void Visit(string settingsNamespace, string fieldPath, FieldInfo defaultsField, object defaults, FieldInfo targetField, object target)
        {
            object targetValue = targetField.GetValue(target);
            if (!IsFieldInitialized(targetField, targetValue))
                targetField.SetValue(target, defaultsField.GetValue(defaults));
        }

        static bool IsFieldInitialized(FieldInfo field, object fieldValue)
        {
            return !field.FieldType.IsValueType && fieldValue != null;
        }
    }
}
