using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace Tam.Maestro.Common.Utilities.Logging
{
    //TODO: We can make use of Lifetime manager in unity container to create singleton instance rather than calling Logger.
    //TODO: Needs dependency analysis.
    /// <summary>
    /// Log Helper with singleton implementation. Use "Log" property to get the instance.
    /// </summary>
    public class LogHelper
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private LogHelper()
        {
        }

        /// <summary>
        /// log 4 net Logger
        /// </summary>
        public static log4net.ILog Logger
        {
            get { return log; }
        }

        /// <summary>
        /// Log helper instance
        /// </summary>
        public static LogHelper Log
        {
            get { return new LogHelper(); }
        }

        /// <summary>
        /// Logs Info as Service call has started
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="methodName">Method name</param>
        /// <param name="userName">logged in user name</param>
        /// <param name="environment">Current environment</param>
        public void ServiceCallStart(string serviceName, string methodName, string userName, string environment)
        {
            var logInfo = new Dictionary<string, string>
            {
                { nameof(environment), environment }
                , { nameof(serviceName), serviceName }
                , { nameof(methodName), methodName }
                , { nameof(userName), userName }
                , { "action", "Service has started" }
            };
            Logger.Info(_GetJsonFormatedString(logInfo));
        }

        /// <summary>
        /// /// Logs warning as Service call has stopped
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="methodName">Method name</param>
        /// <param name="userName">logged in user name</param>
        /// <param name="environment">Current environment</param>
        public void ServiceCallStop(string serviceName, string methodName, string userName, string environment)
        {
            var logWarning = new Dictionary<string, string>
            {
                { nameof(environment), environment }
                , { nameof(serviceName), serviceName }
                , { nameof(methodName), methodName }
                , { nameof(userName), userName }
                , { "action", "Service has stopped" }
            };
            Logger.Warn(_GetJsonFormatedString(logWarning));
        }

        /// <summary>
        /// Logs Info with total duration of service call in seconds.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="methodName">Method name</param>
        /// <param name="totalSeconds">Total duration of service call in seconds</param>
        /// <param name="userName">logged in user name</param>
        /// <param name="environment">Current environment</param>
        public void ServiceCallTotalTime(string serviceName, string methodName, double totalSeconds, string userName, string environment)
        {
            var logInfo = new Dictionary<string, string>
            {
                { nameof(environment), environment }
                , { nameof(serviceName), serviceName }
                , { nameof(methodName), methodName }
                , { nameof(totalSeconds), totalSeconds.ToString(CultureInfo.InvariantCulture) }
                , { nameof(userName), userName }
            };
            Logger.Info(_GetJsonFormatedString(logInfo));
        }

        /// <summary>
        /// Logs Error with message and call stack.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="message">Error message</param>
        /// <param name="callStack">Exception call stack</param>
        /// <param name="userName">logged in user name</param>
        /// <param name="environment">Current environment</param>
        public void ServiceError(string serviceName, string message, string callStack, string userName, string environment)
        {
            var logError = new Dictionary<string, string>
            {
                { nameof(environment), environment }
                , { nameof(serviceName), serviceName }
                , { "exceptionMessage", message }
                , { nameof(callStack), callStack }
                , { nameof(userName), userName }
            };
            Logger.Error(_GetJsonFormatedString(logError));
        }

        /// <summary>
        /// Logs Error with message.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="message">Error message</param>
        /// <param name="userName">logged in user name</param>
        /// <param name="environment">Current environment</param>
        public void ServiceErrorNoCallStack(string serviceName, string message, string userName, string environment)
        {
            var logError = new Dictionary<string, string>
            {
                { nameof(environment), environment }
                , { nameof(serviceName), serviceName }
                , { "exceptionMessage", message }
                , { nameof(userName), userName }
            };
            Logger.Error(_GetJsonFormatedString(logError));
        }

        /// <summary>
        /// Logs Service start event.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="message">Error message</param>
        /// <param name="userName">logged in user name</param>
        /// <param name="environment">Current environment</param>
        public void ServiceEvent(string serviceName, string message, string userName, string environment)
        {
            var logInfo = new Dictionary<string, string>
            {
                { nameof(environment), environment }
                , { nameof(serviceName), serviceName }
                , { nameof(message), message }
                , { nameof(userName), userName }
                , { "action", "Service is started" }
            };
            Logger.Info(_GetJsonFormatedString(logInfo));
        }

        /// <summary>
        /// Logs entity framework entity name error.
        /// </summary>
        /// <param name="entityName">entity name</param>
        /// <param name="entryState">entry state</param>
        public void EntityFrameworkEntityError(string entityName, string entryState)
        {
            var logError = new Dictionary<string, string>
            {
                { nameof(entityName), entityName }
                , { nameof(entryState), entryState }
                , {"action", "Error in Entity name" }
            };
            Logger.Error(_GetJsonFormatedString(logError));
        }

        /// <summary>
        /// Logs entity framework entity property error.
        /// </summary>
        /// <param name="property">property name</param>
        /// <param name="error">error</param>
        public void EntityFrameworkValidationError(string property, string error)
        {
            var logError = new Dictionary<string, string>
            {
                { nameof(property), property }
                , { nameof(error), error }
                , {"action", "Error in Property" }
            };
            Logger.Error(_GetJsonFormatedString(logError));
        }

        #region Private methods
        private static string _GetJsonFormatedString(Dictionary<string, string> data)
        {
            return JsonConvert.SerializeObject(data);
        } 
        #endregion
    }
}
