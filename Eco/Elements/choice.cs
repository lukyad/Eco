using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Elements
{
    /// <summary>
    /// This class is used by the Eco library in conjunction with the Choice attribute.
    /// </summary>
    public class choice<T>
    {
        [Required, Polimorphic]
        public T value;
    }
}
