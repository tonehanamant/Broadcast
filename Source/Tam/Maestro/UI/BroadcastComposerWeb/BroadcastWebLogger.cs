using System;
using Common.Services;
using Common.Services.WebComponents;
using Tam.Maestro.Common.Utilities.Logging;

namespace BroadcastComposerWeb
{
    public class BroadcastWebLogger : IWebLogger
    {
        private IConfiguration _config;

        public BroadcastWebLogger(IConfiguration config)
        {
            _config = config;
        }

        public void LogEventInformation(string message, String serviceName)
        {
            LogHelper.Log.ServiceEvent(serviceName, message, Environment.UserName, _config.EnvironmentName);
        }

        public void LogExceptionWithServiceName(Exception exception, String serviceName)
        {
            LogHelper.Log.ServiceError(serviceName, exception.Message, exception.ToString(), Environment.UserName, _config.EnvironmentName);
        }

        public void LogException(string message, Exception exception, string serviceName)
        {
            // LogException with custom error message
            String exceptionMessage = String.Empty;

            if (!String.IsNullOrEmpty(message))
            {
                exceptionMessage += message + " - ";
            }

            if (exception != null)
            {
                exceptionMessage += exception.Message;
            }

            LogHelper.Log.ServiceError(serviceName, exceptionMessage, exception.ToString(), Environment.UserName, _config.EnvironmentName);
        }
    }
}