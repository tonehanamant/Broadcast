using System;
using Tam.Maestro.Common.Logging;

namespace Common.Services.WebComponents
{
    public interface IWebLogger
    {
        void LogEventInformation(string message, String serviceName);
        void LogException(string message, Exception exception, string serviceName);
        void LogExceptionWithServiceName(Exception exception, String serviceName);
    }

    public class WebLogger : IWebLogger
    {
        private IConfiguration _config;

        public WebLogger(IConfiguration config)
        {
            _config = config;
        }

        public void LogEventInformation(string message, String serviceName)
        {
            TamMaestroEventSource.Log.ServiceEvent(serviceName, message, Environment.UserName, _config.EnvironmentName);
        }

        public void LogExceptionWithServiceName(Exception exception, String serviceName)
        {
            TamMaestroEventSource.Log.ServiceError(serviceName, exception.Message, exception.ToString(), Environment.UserName, _config.EnvironmentName);
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

            TamMaestroEventSource.Log.ServiceError(serviceName, exceptionMessage, exception.ToString(), Environment.UserName, _config.EnvironmentName);
        }
    }
}
