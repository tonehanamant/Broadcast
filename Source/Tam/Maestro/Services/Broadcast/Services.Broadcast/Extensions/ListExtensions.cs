using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Extensions
{
    public static class ListExtentions
    {
        public static bool IsEmpty<T>(this List<T> list)
        {
            return list == null || !list.Any();
        }

        /// <summary>
        /// Groups the elements of a list by a function
        /// </summary>
        /// <typeparam name="T">Type of the element</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="connectionCondition">The connection function.</param>
        /// <returns>List of thehe connected groups </returns>
        public static IEnumerable<IEnumerable<T>> GroupConnected<T>(this IEnumerable<T> list, Func<T, bool> connectionCondition)
        {
            if (list == null)
            {
                yield break;
            }
            using (var enumerator = list.GetEnumerator())
            {
                var temp = new List<T>();
                while (enumerator.MoveNext())
                {
                    T curr = enumerator.Current;
                    {
                        if (connectionCondition(curr))
                        {
                            yield return temp;
                            temp = new List<T>();
                        }
                        if(curr != default)
                        {
                            temp.Add(curr);
                        }                        
                    }
                }
                yield return temp;
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
