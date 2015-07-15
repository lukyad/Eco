using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    class FieldBuilder : FragmentBuilder
    {
        public FieldBuilder(string fieldType, string fieldName)
        {
            base.AddPart(String.Format("public {0} {1};", fieldType, fieldName));
        }
    }
}
