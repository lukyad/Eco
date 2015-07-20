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
		/// This method gets called by the Eco library as a final serialization step to put an instance of
		/// raw settings type to an output stream.
		/// </summary>
		void Serialize(object rawSettings, Stream stream);

		/// <summary>
		/// This method gets called by the Eco library as the very first deserialization step to get an instance of
		/// raw settings type from an input stream.
		/// </summary>
		object Deserialize(Type rawSettingsType, Stream stream);
	}
}
