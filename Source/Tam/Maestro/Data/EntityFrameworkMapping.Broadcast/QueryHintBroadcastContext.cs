using EntityFrameworkMapping.Broadcast.Interceptors;
using System.Data.Entity.Infrastructure.Interception;

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

            _RegisterInterceptors();
        }

        public QueryHintBroadcastContext(string connectionString) : base(connectionString)
        {
            //uncomment this line when you want to see the SQL run by the EF
            //Database.Log = message => Debug.WriteLine(message);

            _RegisterInterceptors();
        }

        private void _RegisterInterceptors()
        {
            DbInterception.Add(new TemporalTableCommandTreeInterceptor());
        }
    }
}
