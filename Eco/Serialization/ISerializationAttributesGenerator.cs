using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco.Serialization
{
	public interface ISerializationAttributesGenerator
	{
		IEnumerable<string> GetAttributesTextFor(Type settingsType, bool isRoot);
		IEnumerable<string> GetAttributesTextFor(string settingsNamespace, FieldInfo settingsField, Usage defaulUsage);
	}
}
