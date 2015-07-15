using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    class FragmentBuilder
    {
		readonly List<string> _attributes = new List<string>();
        readonly List<object> _parts = new List<object>();

        public void AddAttributes(IEnumerable<string> attributes)
        {
			_attributes.AddRange(attributes);
        }

        public void AddPart(object part)
        {
            _parts.Add(part);
        }

        public TBuilder AddPartBuilder<TBuilder>(TBuilder partBuilder)
        {
            _parts.Add(partBuilder);
            return partBuilder;
        }

        public override string ToString()
        {
			var fragment = new StringBuilder();

			foreach (var attribute in _attributes)
				fragment.AppendLine(attribute);

			foreach (var part in _parts)
				fragment.AppendLine(part.ToString());

            return fragment.ToString();
        }
    }
}
