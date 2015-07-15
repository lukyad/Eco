using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    interface IFieldVisitor
    {
        void Visit(FieldInfo sourceField, object sourceSettings, FieldInfo targetField, object targetSettings);
    }
}
