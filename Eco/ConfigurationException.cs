using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    public class ConfigurationException : ApplicationException
    {
        public ConfigurationException(string format, params object[] args) 
            : base(String.Format(format, args))
        {
        }
    }
}
