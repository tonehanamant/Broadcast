using System;
using Tam.Maestro.Common.Logging;

namespace Common.Services.WebComponents
{
    public interface IWebLogger
    {
        void LogEventInformation(string message, String serviceName);
        void LogException(string message, Exception exception, string serviceName);
        void LogExceptionWithServiceName(Exception exception, String serviceName, String requestURL = "");
    }

    public class WebLogger : IWebLogger
    {
        private IConfiguration _config;

        public WebLogger(IConfiguration config)
        {
            _config = config;
        }

        public void LogEventInformation(string message, string serviceName)
        {
            TamMaestroEventSource.Log.ServiceEvent(serviceName, message, Environment.UserName, _config.EnvironmentName);
        }

        public void LogExceptionWithServiceName(Exception exception, string serviceName, string requestURL = "")
        {
            var fullMessage = string.IsNullOrEmpty(requestURL) ? exception.Message : $"{exception.Message} - RequestUrl: {requestURL}";
            TamMaestroEventSource.Log.ServiceError(serviceName, fullMessage, exception.ToString(), Environment.UserName, _config.EnvironmentName);
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

            TamMaestroEventSource.Log.ServiceError(serviceName, exceptionMessage, exception.ToString(), Environment.UserName, _config.EnvironmentName);
        }
    }
}
