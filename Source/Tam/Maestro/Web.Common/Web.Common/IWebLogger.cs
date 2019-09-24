using System;

namespace Common.Services.WebComponents
{
    /// <summary>
    /// Web Logger
    /// </summary>
    public interface IWebLogger
    {
        /// <summary>
        /// Logs event Info
        /// </summary>
        /// <param name="message">Info message</param>
        /// <param name="serviceName">Service name</param>
        void LogEventInformation(string message, string serviceName);

        /// <summary>
        /// Logs exception message.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="exception">exception</param>
        /// <param name="serviceName">service name</param>
        void LogException(string message, Exception exception, string serviceName);

        /// <summary>
        /// Logs exception with service name and request url
        /// </summary>
        /// <param name="exception">exception</param>
        /// <param name="serviceName">service name</param>
        /// <param name="requestURL">request url</param>
        void LogExceptionWithServiceName(Exception exception, string serviceName, string requestURL = "");
    }
}
