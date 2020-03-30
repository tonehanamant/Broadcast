using System.Configuration;
using BroadcastLogging;

namespace BroadcastJobScheduler.Service
{
    /// <inheritdoc />
    public class BroadcastJobSchedulerServiceLogConfig : IBroadcastLoggingConfiguration
    {
        /// <inheritdoc />
        public bool LoggingEnabled
        {
            get
            {
                bool.TryParse(ConfigurationManager.AppSettings["LoggingEnabled"], out var loggingEnabled);
                return loggingEnabled;
            }
        }

        /// <inheritdoc />
        public string LoggingSecureEndpoint
        {
            get
            {
                return ConfigurationManager.AppSettings["LoggingSecureEndpoint"];
            }
        }

        /// <inheritdoc />
        public string LoggingServiceUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["LoggingServiceUrl"] ?? string.Empty;
            }
        }

        /// <inheritdoc />
        public string LoggingUnsecureEndpoint
        {
            get
            {
                return ConfigurationManager.AppSettings["LoggingUnsecureEndpoint"];
            }
        }

        /// <inheritdoc />
        public string LogLevelSetting
        {
            get
            {
                return ConfigurationManager.AppSettings["LogLevelSetting"];
            }
        }

        /// <inheritdoc />
        public string AppName
        {
            get
            {
                return ConfigurationManager.AppSettings["AppName"];
            }
        }

        /// <inheritdoc />
        public string EnvironmentName
        {
            get
            {
                return ConfigurationManager.AppSettings["TAMEnvironment"];
            }
        }

        /// <inheritdoc />
        public string LogFilePath
        {
            get
            {
                return ConfigurationManager.AppSettings["LoggingLogFilePath"];
            }
        }

        /// <inheritdoc />
        public string ApplicationId
        {
            get
            {
                return ConfigurationManager.AppSettings["ApplicationId"];
            }
        }

        /// <inheritdoc />
        public int MaxFileSizeMb
        {
            get
            {
                int.TryParse(ConfigurationManager.AppSettings["MaxFileSizeMb"], out var maxFileSizeMb);
                return maxFileSizeMb;
            }
        }
    }
}