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
    }
}
