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
    public class InlineAttribute : Attribute
    {
        static readonly HashSet<Type> _incompatibleAttributeTypes = new HashSet<Type>
        {
            typeof(RefAttribute)
        };

        public static void ValidateContext(FieldInfo context)
        {
            if (!context.FieldType.IsArray)
            {
                throw new ConfigurationException(
                    "{0} cannot be applied to {1}.{2}. Expected field of an array type",
                    typeof(ChoiceAttribute).Name,
                    context.DeclaringType.Name,
                    context.Name
                );
            }
            AttributeValidator.CheckAttributesCompatibility(context, _incompatibleAttributeTypes);
        }
    }
}
