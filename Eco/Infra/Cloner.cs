using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Eco
{
    public static class Cloner
    {
        static Dictionary<Type, Func<object, object>> _cloneDelegates = new Dictionary<Type, Func<object, object>>();

        public static object Clone(object o)
        {
            if (o == null) return null;

            var type = o.GetType();
            if (!_cloneDelegates.TryGetValue(type, out Func<object, object> clone))
            {
                var input = Expression.Parameter(typeof(object));
                var memberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
                var memberwiseCloneExpression = Expression.Call(input, memberwiseCloneMethod);
                clone = Expression.Lambda<Func<object, object>>(memberwiseCloneExpression, input).Compile();
                _cloneDelegates.Add(o.GetType(), clone);
            }

            return clone(o);
        }
    }
}
