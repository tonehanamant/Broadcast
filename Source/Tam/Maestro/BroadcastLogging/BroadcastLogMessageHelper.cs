using Cadent.Library.Logging.Standard.Common;
using Cadent.Library.Logging.Standard.Common.LoggingModels;
using System;

namespace BroadcastLogging
{
    /// <summary>
    /// Helper classes for working with the logger.
    /// </summary>
    public static class BroadcastLogMessageHelper
    {
        public static IBroadcastLoggingConfiguration Configuration { get; set; } = null;

        public static LogMessage GetApplicationLogMessage(string message, Type callingType, string callingMemberName, string userName = null)
        {
            var additionalInfo = $"{callingType.FullName}.{callingMemberName}";
            var logMessage = _CreateLogMessage(message, LogType.STANDARD, null, additionalInfo, userName);
            return logMessage;
        }

        public static LogMessage GetHttpRequestLogMessage(string message, Guid transactionId, string requestInfo)
        {
            var logMessage = _CreateLogMessage(message, LogType.HTTP_REQUEST, transactionId, null, null);
            return logMessage;
        }

        public static LogMessage GetHttpResponseLogMessage(string message, Guid transactionId, string requestInfo)
        {
            var logMessage = _CreateLogMessage(message, LogType.HTTP_RESPONSE, transactionId, null, null);
            return logMessage;
        }

        private static LogMessage _CreateLogMessage(string payload = null,
            string logType = null,
            Guid? transactionId = null,
            string additionalInfo = null,
            string userName = null
            )
        {
            var message = new LogMessage();
            if (Configuration != null)
            {
                message.Config = new LoggingConfig
                {
                    LoggingEnabled = Configuration.LoggingEnabled,
                    SecureEndpoint = Configuration.LoggingSecureEndpoint,
                    ServiceUrl = Configuration.LoggingServiceUrl,
                    UnsecureEndpoint = Configuration.LoggingUnsecureEndpoint,
                    AppName = Configuration.AppName,
                    ApplicationId = Configuration.ApplicationId,
                    EnvironmentName = Configuration.EnvironmentName,
                    LogFilePath = Configuration.LogFilePath,
                    LogLevelSetting = Configuration.LogLevelSetting,
                    MaxFileSizeMb = Configuration.MaxFileSizeMb,
                };
            }

            message.Data = new AdditionalLogData
            {
                ApplicationId = Configuration?.ApplicationId,
                EnvironmentName = Configuration?.EnvironmentName, 
                TransactionId = transactionId.HasValue ? transactionId.Value.ToString() : string.Empty,
                LogType = logType,
                User = userName ?? "System", 
                Version = "1.0", // TODO: Need to decided if this is hard coded
                Payload = payload,
                AdditionalData = additionalInfo
            };
            return message;
        }
    }
}