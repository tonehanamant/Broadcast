using System.Diagnostics;

namespace EntityFrameworkMapping.Broadcast
{
    public class QueryHintBroadcastContext : BroadcastContext
    {
        public string QueryHint { get; set; }
        public bool ApplyHint { get; set; }

        public QueryHintBroadcastContext() : base()
        {
            //uncomment this line when you want to see the SQL run by the EF
            //Database.Log = message => Debug.WriteLine(message);
        }

        public QueryHintBroadcastContext(string connectionString) : base(connectionString)
        {
            //uncomment this line when you want to see the SQL run by the EF
            //Database.Log = message => Debug.WriteLine(message);
        }
    }
}
