using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Eco.FieldVisitors
{
    class ConfigurationVariableExpander : IFieldVisitor
    {
        public void Visit(string fieldPath, FieldInfo sourceField, object sourceSettings, FieldInfo targetField, object targetSettings)
        {
            throw new NotImplementedException();
        }
    }
}
