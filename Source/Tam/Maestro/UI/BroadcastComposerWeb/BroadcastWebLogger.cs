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

        public void LogEventInformation(string message, string serviceName)
        {
            LogHelper.Log.ServiceEvent(serviceName, message, Environment.UserName, _config.EnvironmentName);
        }

        public void LogExceptionWithServiceName(Exception exception, string serviceName, string requestURL = "")
        {
            LogHelper.Log.ServiceError(serviceName, exception.Message, exception.ToString(), Environment.UserName, _config.EnvironmentName, requestURL);
        }

        public void LogException(string message, Exception exception, string serviceName)
        {
            // LogException with custom error message
            string exceptionMessage = string.Empty;

            if (!string.IsNullOrEmpty(message))
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