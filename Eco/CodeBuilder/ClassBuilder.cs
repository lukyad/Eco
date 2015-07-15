using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    class ClassBuilder : FragmentBuilder
    {
        readonly BlockBuilder _classBody = new BlockBuilder();

        public ClassBuilder(string className, string derivedFrom = null)
        {
            if (derivedFrom != null) derivedFrom = " : " + derivedFrom;
            base.AddPart("public class " + className + derivedFrom);
            base.AddPart(_classBody);
        }

        public FieldBuilder AddField(string type, string name)
        {
            return _classBody.AddPartBuilder(new FieldBuilder(type, name));
        }
    }
}
