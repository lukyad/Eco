using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ConverterAttribute : Attribute
	{
		public ConverterAttribute(Type converterType) 
			: this(converterType, null)
		{
		}

        public ConverterAttribute(Type converterType, string format)
		{
			this.Type = converterType;
			this.Format = format;
			this.ToString = GetToStringMethod(converterType, format);
			this.FromString = GetFromStringMethod(converterType, format);
		}

		public Type Type { get; set; }

		public string Format { get; set; }

		public new Func<object, string> ToString { get; private set; }

		public Func<string, object> FromString { get; private set; }

		static Func<object, string> GetToStringMethod(Type converterType, string format)
		{
			MethodInfo toStringMethod = converterType.GetMethod("ToString", new[] { typeof(string), typeof(object) });
			if (toStringMethod == null || toStringMethod.ReturnType != typeof(string))
				ThrowMissingMethodException(converterType, "string ToString(string format, object value)");
			
			return value => (string)toStringMethod.Invoke(null, new[] { format, value });
        }

		static Func<string, object> GetFromStringMethod(Type converterType, string format)
		{
			MethodInfo fromStringMethod = converterType.GetMethod("FromString", new[] { typeof(string), typeof(string) });
			if (fromStringMethod == null || fromStringMethod.ReturnType != typeof(object))
				ThrowMissingMethodException(converterType, "object FromString(string format, object value)");

			return str => fromStringMethod.Invoke(null, new[] { format, str });
		}

		static void ThrowMissingMethodException(Type methodContainer, string methodSignature)
		{
			throw new ApplicationException(String.Format("{0} type doesn't have required method: {1}", methodContainer.Name, methodSignature));
		}
	}
}
