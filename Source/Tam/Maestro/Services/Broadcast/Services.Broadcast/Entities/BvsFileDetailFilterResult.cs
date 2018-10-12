using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class FileDetailFilterResult<T> where T : class
    {
        public List<T> Ignored = new List<T>();
        public List<T> Updated = new List<T>();
        public List<T> New = new List<T>();
    }
}