//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Reflection;

//namespace Eco
//{
//    /// <summary>
//    /// Base class for all attributes that instruct serializer to change raw type of the field.
//    /// </summary>
//    public abstract class FieldMutatorAttribute : EcoFieldAttribute
//    {
//        readonly Func<FieldInfo, Type> _fieldTypeProvider;
//        readonly Func<FieldInfo, string> _fieldAttributeProvider;
//        readonly Func<FieldInfo, object, object> _fieldValueGetter;
//        readonly Action<FieldInfo, object, object> _fieldValueSetter;

//        public FieldMutatorAttribute(
//            Func<FieldInfo, Type> fieldTypeProvider,
//            Func<FieldInfo, string> fieldAttributeProvider,
//            Func<FieldInfo, object, object> fieldValueGetter, 
//            Action<FieldInfo, object, object> fieldValueSetter)
//        {
//            _fieldTypeProvider = fieldTypeProvider;
//            _fieldAttributeProvider = fieldAttributeProvider;
//            _fieldValueGetter = fieldValueGetter;
//            _fieldValueSetter = fieldValueSetter;
//        }

//        // Given the refined settings filed info returns type of the raw settings field (ie mutated type)
//        public Func<FieldInfo, Type> GetRawSettingsFieldType { get { return _fieldTypeProvider; } }

//        // Given the refined settings field info returns text for custom field attributes
//        // that will be applied to the taw settings field.
//        public Func<FieldInfo, string> GetRawSettingsFieldAttributeText { get { return _fieldAttributeProvider; } }

//        // Given the raw settings field info and an instance of the raw settings object containng the field,
//        // returns a value for the raw settings field.
//        public Func<FieldInfo, object, object> GetRawSettingsFieldValue { get { return _fieldValueGetter; } }

//        // Given the raw settings field info, an instance of the raw settins object containng the field and
//        // a non-mutated value for the field, sets field's value.
//        public Action<FieldInfo, object, object> SetRawSettingsFieldValue { get { return _fieldValueSetter; } }
//    }
//}
