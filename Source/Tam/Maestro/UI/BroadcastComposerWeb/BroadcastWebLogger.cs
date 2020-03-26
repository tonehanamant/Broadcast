using System;
using System.Text;
using Common.Services;
using Common.Services.WebComponents;
using Tam.Maestro.Common.Utilities.Logging;

namespace BroadcastComposerWeb
{
    ///<inheritdoc cref = "IWebLogger" />
    public class BroadcastWebLogger : IWebLogger
    {
        private IConfiguration _config;

        ///<inheritdoc cref = "IWebLogger" />
        public BroadcastWebLogger(IConfiguration config)
        {
            _config = config;  
        }

        ///<inheritdoc cref = "IWebLogger.LogEventInformation" />event
        public void LogEventInformation(string message, string serviceName)
        {
            LogHelper.Log.ServiceEvent(serviceName, message, Environment.UserName, _config.EnvironmentName);
        }

        ///<inheritdoc cref = "IWebLogger.LogExceptionWithServiceName" />event
        public void LogExceptionWithServiceName(Exception exception, string serviceName, string requestURL = "")
        {
            var fullMessage = string.IsNullOrEmpty(requestURL) ? exception.Message : $"{exception.Message} - RequestUrl: {requestURL}";
            LogHelper.Log.ServiceError(serviceName, fullMessage, exception.StackTrace, Environment.UserName, _config.EnvironmentName, requestURL);
        }

        ///<inheritdoc cref = "IWebLogger.LogException" />event
        public void LogException(string message, Exception exception, string serviceName)
        {
            // LogException with custom error message
            var builder = new StringBuilder(message);
            builder.Append(exception?.Message);
            LogHelper.Log.ServiceError(serviceName, builder.ToString(), exception.ToString(), Environment.UserName, _config.EnvironmentName);
        }
    }
}