using Common.Services;
using System.Web.Configuration;
using Tam.Maestro.Services.Clients;

namespace Tam.Maestro.Web.Common
{
    public class FileBasedConfiguration : IConfiguration
    {
        string _environmentName;
        string _logFilePath;

        public FileBasedConfiguration()
        {
            _environmentName = SMSClient.Handler.TamEnvironment.ToString();
            _logFilePath = WebConfigurationManager.AppSettings["logFilePath"];
        }

        public string EnvironmentName
        {
            get { return _environmentName; }
        }

        public string LogFilePath
        {
            get { return _logFilePath; }
        }
    }
}