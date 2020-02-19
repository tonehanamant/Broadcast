using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Extensions
{
    public static class ListExtentions
    {
        public static bool ContainsAny<T>(this IEnumerable<T> list, IEnumerable<T> listToCompareWith)
        {
            return list.Any(x => listToCompareWith.Contains(x));
        }

        public static bool IsEmpty<T>(this List<T> list)
        {
            return list == null || !list.Any();
        }

        /// <summary>
        /// Groups the elements of a list by a function
        /// </summary>
        /// <typeparam name="T1">Type of the element</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="connectedCondition">The grouping function.</param>
        /// <returns>List of thehe connected groups </returns>
        public static IEnumerable<IEnumerable<T1>> GroupConnectedItems<T1>(this IEnumerable<T1> list
            , Func<T1, T1, bool> connectedCondition)
        { 
            if (!list.Any())
            {
                yield break;
            }
            using (var enumerator = list.GetEnumerator())
            {
                var temp = new List<T1>();
                T1 previous = default;
                while (enumerator.MoveNext())
                {
                    T1 current = enumerator.Current;
                    {
                        if (connectedCondition(previous, current))
                        {
                            if(temp.Count > 0)
                            {
                                yield return temp;
                            }                            
                            temp = new List<T1>();
                        }
                        if (current != default)
                        {
                            temp.Add(current);
                        }
                        previous = current;
                    }
                }
                if (temp.Count > 0)
                {
                    yield return temp;
                }
            }
        }

        public static IEnumerable<List<T>> GetChunks<T>(this List<T> items, int chunkSize)
        {
            for (int i = 0; i < items.Count; i += chunkSize)
            {
                yield return items.GetRange(i, Math.Min(chunkSize, items.Count - i));
            }
        }
    }
}
