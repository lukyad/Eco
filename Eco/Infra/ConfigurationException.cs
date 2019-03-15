using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    /// <summary>
    /// Type for all exceptions raised by the Eco library.
    /// </summary>
    public class ConfigurationException : ApplicationException
    {
        public ConfigurationException(string format, params object[] args) 
            : base(String.Format(format, args))
        {
        }

        public ConfigurationException(Exception innerException, string format, params object[] args)
            : base(String.Format(format, args), innerException)
        {
        }
    }
}
