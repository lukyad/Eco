using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Elements;

namespace Eco
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChoiceAttribute : FieldMutatorAttribute
    {
        public ChoiceAttribute()
            : base(GetRawSettingsFieldType, GetRawSettingsFieldAttributeText, GetRawSettingsFieldValue, SetRawSettingsFieldValue)
        {
        }

        public void ValidateContext(FieldInfo context)
        {
        }

        static new Type GetRawSettingsFieldType(FieldInfo refinedSettingsField)
        {
            return typeof(choice<>).MakeGenericType(refinedSettingsField.FieldType);
        }

        static new string GetRawSettingsFieldAttributeText(FieldInfo refinedSettingsField)
        {
            return null;
        }

        static new object GetRawSettingsFieldValue(FieldInfo rawSettingsField, object rawSettings)
        {
            object choice = rawSettingsField.GetValue(rawSettings);
            if (choice == null) return null;
            return GetValueField(choice).GetValue(choice);
        }

        static new void SetRawSettingsFieldValue(FieldInfo rawSettingsField, object rawSettings, object nonMutatedRawSettingsValue)
        {
            object choice = null;
            if (nonMutatedRawSettingsValue != null)
            {
                choice = Activator.CreateInstance(rawSettingsField.FieldType);
                GetValueField(choice).SetValue(choice, nonMutatedRawSettingsValue);
            }
            rawSettingsField.SetValue(rawSettings, choice);
        }

        static FieldInfo GetValueField(object choice)
        {
            return choice.GetType().GetField("value");
        }
    }
}
