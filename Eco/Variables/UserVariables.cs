using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Eco
{
    public class UserVariables : IVariableProvider
    {
        static readonly Dictionary<string, Func<string>> _variables = new Dictionary<string, Func<string>>();

        public static void Add(string name, string value) => Add(name, () => value);

        public static void Add(string name, Func<string> valueProvider)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (valueProvider == null)
                throw new ArgumentNullException(nameof(valueProvider));

            if (_variables.ContainsKey(name))
                throw new ArgumentException($"User variable with the same name already exists: `{name}`");

            _variables.Add(name, valueProvider);
        }

        public Dictionary<string, Func<string>> GetVariables() => _variables;
    }
}
