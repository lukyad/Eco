using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ItemNameAttribute : Attribute
    {
        public ItemNameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
