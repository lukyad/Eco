using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{ 
    /// <summary>
    /// Eco configuration library supports string varibales.
    /// You can enable variables in your configuration file by adding 'public variable[] variables;'
    /// field to your root configuration type. 
    /// Variable can be referenced anywhere in a configuration file by it's name using the following syntax: ${name}.
    /// 
    /// Variable's value can reference another variable. In this case variable value is expanded recursively. 
    /// Eco library throws an exception if a circular variable dendency is detected.
    /// 
    /// Variables have a global scope, i.e. visible at any place in the configuration file where they are defined
    /// as well as in any included configuration file.
    /// </summary>
    [EcoElement(typeof(variable))]
    [Doc("Represents a configuration variable of the string type. Can be referenced anywhere in a configuration file by the following syntax: ${name}.")]
    public sealed class variable
    {
        [Required, Doc("Name of the varible. Can contain 'word' characters only (ie [A-Za-z0-9_]).")]
        public string name;

        [Required, Sealed, Doc("Variable's value.")]
        public string value;

        public static string GetValue(object twin) => (string)twin.GetType().GetField(nameof(value)).GetValue(twin);

        public static string GetName(object twin) => (string)twin.GetType().GetField(nameof(name)).GetValue(twin);
    }
}
