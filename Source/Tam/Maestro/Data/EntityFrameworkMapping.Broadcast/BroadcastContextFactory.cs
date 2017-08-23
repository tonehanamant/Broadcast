using System;
using System.Data.Entity.Infrastructure.Interception;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;

namespace EntityFrameworkMapping.Broadcast
{
    public interface IBroadcastContextFactory : IContextFactory<QueryHintBroadcastContext>
    {
    }

    public class BroadcastContextFactory : IBroadcastContextFactory
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

        static BroadcastContextFactory()
        {
            DbInterception.Add(new HintInterceptor());
        }

        private void ThisIsNeededToSolveARunTimeIssue_ForSomeReasonADllNeededByEntityFrameworkWontBeLoadedOtherwise()
        {
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        public QueryHintBroadcastContext FromTamConnectionString(string tamConnectionString)
        {
            var connectionString = ConnectionStringHelper.BuildConnectionString(tamConnectionString, ApplicationName);

            var connString = @"metadata=res://*/BroadcastContext.csdl|res://*/BroadcastContext.ssdl|res://*/BroadcastContext.msl;provider=System.Data.SqlClient;provider connection string=""{0}"" ";

            var entityFrameworkConnectionString = String.Format(connString, connectionString);
            return new QueryHintBroadcastContext(entityFrameworkConnectionString);
        }
    }
}
