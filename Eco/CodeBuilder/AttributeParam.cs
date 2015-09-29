using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.CodeBuilder
{
    class AttributeParam
    {
        public AttributeParam(string value)
        {
            Value = value;
        }

        public AttributeParam(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string Value { get; }

        public override string ToString() => Name != null ? $"{Name} = {Value}" : Value;
    }
}
