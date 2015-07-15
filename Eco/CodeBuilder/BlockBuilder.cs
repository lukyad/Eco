using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    class BlockBuilder : FragmentBuilder
    {
        public override string ToString()
        {
            return
                new StringBuilder()
                .AppendLine("{")
                .AppendLine(base.ToString())
                .AppendLine("}")
                .ToString();
        }
    }
}
