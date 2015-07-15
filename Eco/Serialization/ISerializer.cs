using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Eco.Serialization
{
	public interface ISerializaer
	{
		void Serialize(object rawSettings, Stream stream);
		object Deserialize(Type rawSettingsType, Stream stream);
	}
}
