using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkMapping.Broadcast
{
    public class QueryHintBroadcastContext : BroadcastContext
    {
        public string QueryHint { get; set; }
        public bool ApplyHint { get; set; }

        public QueryHintBroadcastContext() : base() { }

        public QueryHintBroadcastContext(string connectionString) : base(connectionString) { }
    }
}
