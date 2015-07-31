using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco.Serialization
{
    /// <summary>
    /// Allows customization of the serializable settings type generation process.
    /// Pass an instance of your custom implementation of ISerializationAttributesGenerator interface to the SettingsManager object
    /// in order to generate your custom serialization attributes that can be used by your custom Serializer.
    /// </summary>
    public interface ISerializationAttributesGenerator
    {
        /// <summary>
        /// This method gets called by the Eco library for each new settings type being serialized.
        /// It should return a text reperesentation of all type attributes required by your custom serializer.
        /// The result string list should be compilable by c# compiler.
        /// 
        /// An example string list might look as follows:
        /// 
        /// [System.Runtime.Serialization.DataContractAttribute()]
        /// [Eco.DocAttribute("Annotation")]
        /// [MyNamesapce.MyCustomTypeAttribute()]
        /// </summary>
        IEnumerable<string> GetAttributesTextFor(Type settingsType);

        /// <summary>
        /// This method gets called by the Eco library for each field of the serializable settings type.
        /// It should return a text reperesentation of all field attributes required by your custom serializer.
        /// The result string list should be compilable by c# compiler.
        /// 
        /// An example string list might look as follows:
        /// 
        /// [System.Runtime.Serialization.DataMemberAttribute(Name = "CustomName")]
        /// [Eco.SealedAttribute()]
        /// [MyNamesapce.MyCustomFieldAttribute()]
        /// </summary>
        IEnumerable<string> GetAttributesTextFor(FieldInfo settingsField, Usage defaulUsage);
    }
}
