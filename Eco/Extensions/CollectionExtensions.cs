using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Extensions
{
    static class CollectionExtensions
    {
        public static T[] Union<T>(this T[] first, T[] second)
        {
            if (first == null) return second;
            if (second == null) return first;

            var res = new T[first.LongLength + second.LongLength];
            first.CopyTo(res, 0L);
            second.CopyTo(res, first.LongLength);
            return res;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> collection, T item)
        {
            foreach (var i in collection)
                yield return i;

            yield return item;
        }

        public static IEnumerable<T> Replace<T>(this IEnumerable<T> collection, Func<T, bool> predicate, T newItem)
        {
            foreach (var item in collection)
            {
                if (predicate(item))
                    yield return newItem;
                else 
                    yield return item;
            }
        }
    }
}
