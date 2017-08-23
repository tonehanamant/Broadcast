using System.Data.SqlClient;

namespace Tam.Maestro.Common
{
    public class ConnectionStringHelper
    {
        public static string BuildConnectionString(string connectionStringFromConfig, string applicationName)
        {
            var isIntegratedSecurity = connectionStringFromConfig.Contains("Integrated Security=SSPI");
            if (isIntegratedSecurity)
            {

                return connectionStringFromConfig + ";Application Name=" + applicationName;
            }
            else
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(connectionStringFromConfig);
                connectionStringBuilder.Password = EncryptionHelper.DecryptString(connectionStringBuilder.Password, EncryptionHelper.EncryptionKey);
                connectionStringBuilder.ApplicationName = applicationName;
                return connectionStringBuilder.ToString();
            }
        }
    }
}