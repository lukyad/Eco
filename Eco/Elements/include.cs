using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco
{
    /// <summary>
    /// Instructs serializer to include the specified configuration file.
    /// The include element can be used inly with conjunction with the KnownTypes attribute.
    /// </summary>
    [IncludeElement, RequiredAttributes(typeof(KnownTypesAttribute))]
    [Doc("Specifies an external file to be included.")]
    public abstract class include
    {
        [Required, Doc("Path (relative or absolute) to the file to be included.")]
        public string file;

        [Optional, Doc("Namespace to be applied to all object IDs included from the referenced file. If specified, an object from the file should be referenced as '<Namesapce>.<ObjectId>'")]
        public string namesapce;

        [Optional, Doc("Format of the file to be included. By default Eco library uses format defined by Settings.DefaultManager.")]
        public string format;
    }

    /// <summary>
    /// Instructs serializer to include the specified configuration file.
    /// </summary>
    [IncludeElement, Doc("Specifies an external file to be included.")]
    public sealed class include<T> : include
    {
        [Ignore]
        public T data;
    }
}
