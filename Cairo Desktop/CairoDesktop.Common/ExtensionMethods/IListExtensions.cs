using System.Collections.Generic;

namespace CairoDesktop.Common.ExtensionMethods
{
    public static class ListExtensions
    {
        public static void AddRange<TSource>(this IList<TSource> source, params TSource[] items) 
        {
            foreach (TSource item in items)
            {
                source.Add(item);
            }
        }

        public static void RemoveFrom<TSource, TSource2>(this IEnumerable<TSource> source, IList<TSource2> destination) where TSource : TSource2
        {
            foreach (TSource sourceItem in source)
            {
                destination.Remove(sourceItem);
            }
        }

        public static void AddTo<TSource, TSource2>(this IEnumerable<TSource> source, IList<TSource2> destination) where TSource : TSource2
        {
            foreach (TSource sourceItem in source)
            {
                destination.Add(sourceItem);
            }
        }
    }
}