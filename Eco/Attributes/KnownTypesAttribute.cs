using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Eco.Extensions;

namespace Eco
{
    [AttributeUsage(AttributeTargets.Field)]
    public class KnownTypesAttribute : Attribute
    {
        private readonly Type[] _ctorTypes;

        public KnownTypesAttribute()
            : this(new Type[0])
        {
        }

        public KnownTypesAttribute(params Type[] types)
        {
            _ctorTypes = types;
        }

        public string Wildcard { get; set; }

        public IEnumerable<Type> GetAllKnownTypes(FieldInfo context)
        {
            foreach (var t in _ctorTypes) 
                yield return t;

            if (!String.IsNullOrEmpty(this.Wildcard))
            {
                var regexp = new Wildcard(this.Wildcard);
                Func<Type, bool> MatchesWildcard = t => regexp.Match(t.FullName).Success;
				var knownSettingsTypes = context.DeclaringType.Assembly.GetTypes().Where(t => MatchesWildcard(t) && t.IsSettingsType());
				foreach (var t in knownSettingsTypes)
                    yield return t;
            }
        }
    }
}
