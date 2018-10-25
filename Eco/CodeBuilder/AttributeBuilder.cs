using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eco.Extensions;

namespace Eco.CodeBuilder
{
   class AttributeBuilder
    {
        readonly List<AttributeParam> _params = new List<AttributeParam>();
        readonly string _type;
        readonly bool _isAssembly;
        bool _namedParamAdded = false;

        public AttributeBuilder(string attributeType, bool isAssembly = false)
        {
            _type = attributeType;
            _isAssembly = isAssembly;
        }

        public AttributeBuilder AddParam(string value)
        {
            if (_namedParamAdded) throw new ConfigurationException("Unnamed atribute parameter can not be placed after a named one.");
            _params.Add(new AttributeParam(value));
            return this;
        }

        public AttributeBuilder AddParam(string name, string value)
        {
            _params.Add(new AttributeParam(name, value));
            _namedParamAdded = true;
            return this;
        }

        public AttributeBuilder AddTypeParam(string typeName)
        {
            return AddParam(TypeOf(typeName));
        }

        public AttributeBuilder AddTypeParam(string paramName, string typeName)
        {
            return AddParam(paramName, TypeOf(typeName));
        }

        public AttributeBuilder AddStringParam(string value)
        {
            return AddParam(Quoted(value));
        }

        public AttributeBuilder AddStringParam(string paramName, string value)
        {
            return AddParam(paramName, Quoted(value));
        }

        public static string GetTextFor<TAttribute>(params object[] args)
        {
            return GetTextFor(typeof(TAttribute), args);
        }

        public static string GetTextFor(Type attributeType, params object[] args)
        {
            var builder = new AttributeBuilder(attributeType.FullName);
            foreach (object arg in args)
            {
                if (arg is Type) builder.AddTypeParam((arg as Type).FullName);
                else if (arg is string) builder.AddStringParam(arg.ToString());
                else builder.AddParam(arg.ToString());
            }
            return builder.ToString();
        }

        public override string ToString() => $"[{(_isAssembly ? "assembly: " : null)}{_type}({AttributeParams})]";

        string AttributeParams => String.Join(", ", _params);

        static string TypeOf(string typeName) => $"typeof({typeName})";

        static string Quoted(string value) => $"\"{value}\"";
    }
}
