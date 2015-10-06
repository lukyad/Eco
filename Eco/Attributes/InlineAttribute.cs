using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    /// <summary>
    /// Instructs serializer to inline the given array field.
    /// 
    /// XML Example:
    /// 
    /// class myItem
    /// {
    /// }
    /// 
    /// class mySettings
    /// {
    ///        public myItem[] items;
    /// }
    /// 
    /// If you serialize instance of mySettings class to xml you will get the following output:
    /// 
    /// <mySettings>
    ///   <items>
    ///     <myItem/>
    ///     ...
    ///   </items>
    /// </mySettings>
    /// 
    /// Now if you apply InlineAttribute to the items field ie
    /// 
    /// class mySettings
    /// {
    ///        [Inline]
    ///        public myItem[] items;
    /// }
    /// 
    /// you will get the following serialization output:
    /// 
    /// <mySettings>
    ///   <myItem/>
    ///   ...
    /// </mySettings>
    /// 
    /// ie <items> node is inlined now.
    /// 
    /// Usage: 
    /// Can be applied to a field of an array type only.
    /// 
    /// Compatibility:
    /// Incompatible with the Ref attribute and compatible with all others.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InlineAttribute : EcoFieldAttribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(ChoiceAttribute),
            typeof(ConverterAttribute),
            typeof(ExternalAttribute),
            typeof(IdAttribute),
            typeof(ParserAttribute),
            typeof(PolimorphicAttribute),
            typeof(RefAttribute),
        };

        public override void ValidateContext(FieldInfo context)
        {
            if (!context.FieldType.IsArray)
                ThrowExpectedFieldOf("an array type", context);

            CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
