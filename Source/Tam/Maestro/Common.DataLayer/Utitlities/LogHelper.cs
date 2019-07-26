using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Tam.Maestro.Common.Utilities.Logging
{
    public class LogHelper
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private LogHelper()
        {
        }

        public static log4net.ILog Logger
        {
            get { return log; }
        }

        public static LogHelper Log
        {
            get { return new LogHelper(); }
        }


        public  void ServiceCallStart(string serviceName, string methodName, string userName, string environment)
        {
            Logger.InfoFormat("Environment: {3}--> Service Name: {0} is started - MethodName :{1} UserName :{2}", serviceName,methodName,userName,environment);

        }

        public void ServiceCallStop(string serviceName, string methodName, string userName, string environment)
        {
            Logger.WarnFormat("Environment: {3}--> Service Name: {0} is stopped - MethodName :{1} UserName :{2}", serviceName, methodName, userName, environment);
        }

        public void ServiceCallTotalTime(string serviceName, string methodName, double totalSeconds, string userName, string environment)
        {
            Logger.InfoFormat("Environment: {4}--> Service Name: {0} - MethodName :{1}  Duration of service call - {2} UserName :{3}", serviceName, methodName, totalSeconds,userName, environment);
        }


        public void ServiceError(string serviceName, string message, string callStack, string userName, string environment)
        {
            Logger.ErrorFormat("Environment: {4}--> Service Name: {0} - Exception Message :{1}  Callstack - {2} UserName :{3}", serviceName, message, callStack, userName, environment);
        }


        public void ServiceErrorNoCallStack(string serviceName, string message, string userName, string environment)
        {
            Logger.ErrorFormat("Environment: {3}--> Service Name: {0} - Exception Message :{1}  C UserName :{3}", serviceName, message, userName, environment);
        }

        public void ServiceEvent(string serviceName, string message, string userName, string environment)
        {
            Logger.InfoFormat("Environment: {3}--> Service Name: {0} is started - Message :{1} UserName :{2}", serviceName, message, userName, environment);
        }

        public void EntityFrameworkEntityError(string entityName, string entryState)
        {
            Logger.ErrorFormat("Error in Entity Name {0} Entry State -{1}" ,entityName, entryState);
        }

        public void EntityFrameworkValidationError(string property, string error)
        {
            Logger.ErrorFormat("Error in Property  {0} Error -{1}", property, error);
        }
    }
}
