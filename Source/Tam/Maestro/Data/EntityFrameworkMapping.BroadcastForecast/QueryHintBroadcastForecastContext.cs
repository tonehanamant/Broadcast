namespace Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast
{
    public class QueryHintBroadcastForecastContext : BroadcastForecastContext
    {
        public string QueryHint { get; set; }
        public bool ApplyHint { get; set; }

        public QueryHintBroadcastForecastContext() { }

        public QueryHintBroadcastForecastContext(string connectionString) : base(connectionString) { }
    }
}
