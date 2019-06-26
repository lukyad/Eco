using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Eco.Serialization
{
    /// <summary>
    /// Allows importing data from any source.
    /// Works only in one direction, i.e settings reading, writing is not supported.
    /// Pls note, the the implementation class, referenced by the import<,> element must be non-generic
    /// (base class is allowed to be a generic type)
    /// </summary>
    public interface IImporter
    {
        /// <summary>
        /// This method gets called by the Eco library as the very first import step to get an instance of a raw settings type.
        /// All parameters (if any) are passed to the Importer c'tor as an instance of the import<,> element. 
        /// For additional details pls check import<,> class definition.
        /// </summary>
        object Import();
    }
}
