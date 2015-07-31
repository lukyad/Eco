using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    public abstract class FieldMutatorAttribute : Attribute
    {
        readonly Func<FieldInfo, Type> _fieldTypeProvider;
        readonly Func<FieldInfo, string> _fieldAttributeProvider;
        readonly Func<FieldInfo, object, object> _fieldValueGetter;
        readonly Action<FieldInfo, object, object> _fieldValueSetter;

        public FieldMutatorAttribute(
            Func<FieldInfo, Type> fieldTypeProvider,
            Func<FieldInfo, string> fieldAttributeProvider,
            Func<FieldInfo, object, object> fieldValueGetter, 
            Action<FieldInfo, object, object> fieldValueSetter)
        {
            _fieldTypeProvider = fieldTypeProvider;
            _fieldAttributeProvider = fieldAttributeProvider;
            _fieldValueGetter = fieldValueGetter;
            _fieldValueSetter = fieldValueSetter;
        }

        public Func<FieldInfo, Type> GetRawSettingsFieldType { get { return _fieldTypeProvider; } }

        public Func<FieldInfo, string> GetRawSettingsFieldAttributeText { get { return _fieldAttributeProvider; } }

        public Func<FieldInfo, object, object> GetRawSettingsFieldValue { get { return _fieldValueGetter; } }

        public Action<FieldInfo, object, object> SetRawSettingsFieldValue { get { return _fieldValueSetter; } }
    }
}
