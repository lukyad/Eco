using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Variables
{
    /// <summary>
    /// Adds limited support for dynamic variables. 
    /// By the contract any class implementing this interface must have a default constructor.
    /// </summary>
    public interface IVariableProvider
    {
        /// <summary>
        /// Invoked each time on configuration is loaded/saved.
        /// Returns dictionary of variable's name & value pairs actual for the current moment.
        /// </summary>
        Dictionary<string, string> GetVariables();
    }
}
