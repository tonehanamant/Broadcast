using System.Data.Entity.Infrastructure.Interception;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;

namespace Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast
{
    public interface IBroadcastForecastContextFactory : IContextFactory<QueryHintBroadcastForecastContext>
    {
    }

    public class BroadcastForecastContextFactory : IBroadcastForecastContextFactory
    {
        private static volatile string _ApplicationName = null;
        public static string ApplicationName
        {
            get
            {
                if (string.IsNullOrEmpty(_ApplicationName))
                    _ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                return _ApplicationName;
            }
        }

        static BroadcastForecastContextFactory()
        {
            DbInterception.Add(new HintInterceptor());
        }

        private void ThisIsNeededToSolveARunTimeIssue_ForSomeReasonADllNeededByEntityFrameworkWontBeLoadedOtherwise()
        {
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        public QueryHintBroadcastForecastContext FromTamConnectionString(string tamConnectionString)
        {
            var connectionString = ConnectionStringHelper.BuildConnectionString(tamConnectionString, ApplicationName);

            var connString = @"metadata=res://*/BroadcastForecastContext.csdl|res://*/BroadcastForecastContext.ssdl|res://*/BroadcastForecastContext.msl;provider=System.Data.SqlClient;provider connection string=""{0}"" ";

            var entityFrameworkConnectionString = string.Format(connString, connectionString);
            return new QueryHintBroadcastForecastContext(entityFrameworkConnectionString);
        }
    }
}