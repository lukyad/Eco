using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Elements
{
    /// <summary>
    /// Eco configuration library supports string varibales.
    /// You can enable variables in your configuration file by adding 'public variable[] variables;'
    /// field to your root configuration type. 
    /// Variable can be referenced anywhere in a configuration file by it's name using the following syntax: ${name}.
    /// 
    /// Variable's value can reference another variable. In this case variable value is expanded recursively. 
    /// Eco library throws an exception if a circular variable dendency is detected.
    /// </summary>
    [Doc("Represents a configuration variable of the string type. Can be referenced anywhere in a configuration file by the following syntax: ${name}.")]
    public class variable
    {
        [Required, Doc("Name of the varible. Can contain 'word' characters only.")]
        public string name;

        [Required, Doc("Variable's value.")]
        public string value;
    }
}
