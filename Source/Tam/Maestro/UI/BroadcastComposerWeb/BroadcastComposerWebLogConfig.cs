using BroadcastLogging;
using System.Web.Configuration;

namespace BroadcastComposerWeb
{
    /// <summary>
    /// The logging configuration for the application.
    /// </summary>
    /// <seealso cref="BroadcastLogging.IBroadcastLoggingConfiguration" />
    public class BroadcastComposerWebLogConfig : IBroadcastLoggingConfiguration
    {
        /// <inheritdoc />
        public bool LoggingEnabled
        {
            get
            {
                bool.TryParse(WebConfigurationManager.AppSettings["LoggingEnabled"], out var loggingEnabled);
                return loggingEnabled;
            }
        }

        /// <inheritdoc />
        public string LoggingSecureEndpoint
        {
            get
            {
                return WebConfigurationManager.AppSettings["LoggingSecureEndpoint"];
            }
        }

        /// <inheritdoc />
        public string LoggingServiceUrl
        {
            get
            {
                return WebConfigurationManager.AppSettings["LoggingServiceUrl"] ?? string.Empty;
            }
        }

        /// <inheritdoc />
        public string LoggingUnsecureEndpoint
        {
            get
            {
                return WebConfigurationManager.AppSettings["LoggingUnsecureEndpoint"];
            }
        }

        /// <inheritdoc />
        public string LogLevelSetting
        {
            get
            {
                return WebConfigurationManager.AppSettings["LogLevelSetting"];
            }
        }

        /// <inheritdoc />
        public string AppName
        {
            get
            {
                return WebConfigurationManager.AppSettings["AppName"];
            }
        }

        /// <inheritdoc />
        public string EnvironmentName
        {
            get
            {
                return WebConfigurationManager.AppSettings["TAMEnvironment"];
            }
        }

        /// <inheritdoc />
        public string LogFilePath
        {
            get
            {
                return WebConfigurationManager.AppSettings["LoggingLogFilePath"];
            }
        }

        /// <inheritdoc />
        public string ApplicationId
        {
            get
            {
                return WebConfigurationManager.AppSettings["ApplicationId"];
            }
        }

        /// <inheritdoc />
        public int MaxFileSizeMb
        {
            get
            {
                int.TryParse(WebConfigurationManager.AppSettings["MaxFileSizeMb"], out var maxFileSizeMb);
                return maxFileSizeMb;
            }
        }
    }
}