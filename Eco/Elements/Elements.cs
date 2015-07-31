using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Elements
{
    public class choice<T>
    {
        [Required, Polimorphic]
        public T value;
    }

    public class include
    {
        public string file;

        public string format;
    }

    public class variable
    {
        public string name;

        public string value;
    }

}
