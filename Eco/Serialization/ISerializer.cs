using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Eco.Serialization
{
    /// <summary>
    /// Allows custom serialization of your settings types.
    /// Pass an instance of your custom implementation of ISerializer interface to the SettingsManager object
    /// in order to be able load/save your settings in your custom format.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// This method gets called by the Eco library as a final serialization step to write an instance of
        /// a raw settings type using the specified TextWriter.
        /// </summary>
        void Serialize(object rawSettings, TextWriter stream);

        /// <summary>
        /// This method gets called by the Eco library as the very first deserialization step to read an instance of
        /// a raw settings type using the specified TextReader.
        /// </summary>
        object Deserialize(Type rawSettingsType, TextReader stream);
    }
}
