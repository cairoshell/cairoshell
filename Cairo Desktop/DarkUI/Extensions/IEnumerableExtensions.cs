using System.Collections.Generic;
using System.Linq;

namespace DarkUI.Extensions
{
    internal static class IEnumerableExtensions
    {
        internal static bool IsLast<T>(this IEnumerable<T> items, T item)
        {
            var last = items.LastOrDefault();
            if (last == null)
                return false;
            return item.Equals(last);
        }

        internal static bool IsFirst<T>(this IEnumerable<T> items, T item)
        {
            var first = items.FirstOrDefault();
            if (first == null)
                return false;
            return item.Equals(first);
        }

        internal static bool IsFirstOrLast<T>(this IEnumerable<T> items, T item)
        {
            return items.IsFirst(item) || items.IsLast(item);
        }
    }
}
