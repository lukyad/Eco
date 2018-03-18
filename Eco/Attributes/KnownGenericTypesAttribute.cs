using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;

namespace Eco
{
    /// <summary>
    /// Provides additional constructors to the KnownTypesAttributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class KnownGenericTypesAttribute : KnownTypesAttribute
    {
        public KnownGenericTypesAttribute(Type genericTypeDefinition, string genericArgumentWildcard, Type context)
        {
            if (genericTypeDefinition == null) throw new ArgumentNullException(nameof(genericTypeDefinition));
            if (!genericTypeDefinition.IsGenericTypeDefinition) throw new ArgumentException("Invalid argument. Expected a generic type definition.");
            if (genericArgumentWildcard == null) throw new ArgumentNullException(nameof(genericArgumentWildcard));
            if (context == null) throw new ArgumentNullException(nameof(context));
            base.KnownTypes = MatchTypes(genericArgumentWildcard, context.Assembly).Select(t => genericTypeDefinition.MakeGenericType(t)).ToArray();
        }

        public KnownGenericTypesAttribute(Type genericTypeDefinition, Type genericArgumentBase)
        {
            if (genericTypeDefinition == null) throw new ArgumentNullException(nameof(genericTypeDefinition));
            if (!genericTypeDefinition.IsGenericTypeDefinition) throw new ArgumentException("Invalid argument. Expected a generic type definition.");
            if (genericArgumentBase == null) throw new ArgumentNullException(nameof(genericArgumentBase));
            base.KnownTypes = genericArgumentBase.GetDerivedTypes().Append(genericArgumentBase).ToArray();
        }
    }
}
