using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Elements
{
    /// <summary>
    /// This class is used internally by the Eco library.
    /// In order to instruct Eco serializer to load a value of a cirtain field from an external file,
    /// mark the field with the External attribute.
    /// </summary>
    [Doc("Specifies an external file to be included.")]
    public class include
    {
        [Required, Doc("Path (relative or absolute) to the file to be included.")]
        public string file;

        [Optional, Doc("Format of the file to be included. By default Eco library uses format defined by Settings.DefaultManager.")]
        public string format;
    }
}
