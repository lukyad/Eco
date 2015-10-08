using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Elements;
using Eco.Extensions;

namespace Eco
{
    /// <summary>
    /// Instructs serializer to wrap the given field into the choice<TFieldType> object.
    /// The choice element contains a single polimorfic field. Usefull for XML serialization.
    /// 
    /// Usage: 
    /// Can be applied to a field of a settings type only.
    /// 
    /// Compatibility: 
    /// Incompatible with the Id, Inline, ItemName, Converter, External and Ref attributes and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ChoiceAttribute : FieldMutatorAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ConverterAttribute),
            typeof(ExternalAttribute),
            typeof(InlineAttribute),
            typeof(RenameAttribute),
            typeof(KnownTypesAttribute),
            typeof(PolimorphicAttribute),
            typeof(RefAttribute)
        };

        public ChoiceAttribute()
            : base(GetRawSettingsFieldType, GetRawSettingsFieldAttributeText, GetRawSettingsFieldValue, SetRawSettingsFieldValue)
        {
        }

        public override void ValidateContext(FieldInfo context)
        {
            if (!context.FieldType.IsSettingsType() || !context.FieldType.IsAbstract)
                base.ThrowExpectedFieldOf("a settings type", context);

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
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
