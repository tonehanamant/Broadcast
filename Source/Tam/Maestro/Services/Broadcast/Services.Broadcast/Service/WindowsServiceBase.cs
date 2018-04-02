using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.ServiceModel;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using Tam.Maestro.Common.Logging;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Services
{
    public abstract class WindowsServiceBase
    {

        protected WindowsServiceBase()
        {
        }
        public abstract string ServiceName { get; }
        public bool IsConsole { get; set; }

        protected void TimeResultsNoReturn(String methodName, Action func)
        {
            var userName = _GetWindowsUserName();
            var environment = SMSClient.Handler.TamEnvironment;

            TamMaestroEventSource.Log.ServiceCallStart(ServiceName, methodName, userName, environment);

            var stopWatch = Stopwatch.StartNew();
            func();
            stopWatch.Stop();

            TamMaestroEventSource.Log.ServiceCallStop(ServiceName, methodName, userName, environment);
            TamMaestroEventSource.Log.ServiceCallTotalTime(ServiceName, methodName, stopWatch.Elapsed.TotalSeconds, userName, environment);
        }


        protected T TimeResults<T>(String methodName, Func<T> func)
        {
            var userName = _GetWindowsUserName();
            var environment = SMSClient.Handler.TamEnvironment;

            TamMaestroEventSource.Log.ServiceCallStart(ServiceName, methodName, userName, environment);

            var stopWatch = Stopwatch.StartNew();
            var results = func();
            stopWatch.Stop();

            TamMaestroEventSource.Log.ServiceCallStop(ServiceName, methodName, userName, environment);
            TamMaestroEventSource.Log.ServiceCallTotalTime(ServiceName, methodName, stopWatch.Elapsed.TotalSeconds, userName, environment);

            return results;
        }

        public void LogServiceError(string message, Exception exception)
        {
            if (IsConsole)
            {
                message = string.Format("Service Error {0} :: {1}\\n{2}", ServiceName, message,exception);
                Console.WriteLine(message);
            }
            LogServiceError(ServiceName,message,exception);
        }

        public static void LogServiceError(string serviceName, string message, Exception exception)
        {
            var userName = String.Empty;
            var environment = String.Empty;
            try
            {
                userName = _GetWindowsUserName();
                environment = SMSClient.Handler.TamEnvironment;
            }
            catch
            {
            }
            TamMaestroEventSource.Log.ServiceError(serviceName, exception.Message, exception.ToString(), userName, environment);
        }

        public void LogServiceErrorNoCallStack(string message)
        {
            if (IsConsole)
            {
                message = string.Format("Service NoCallStack {0} :: {1}", ServiceName, message);
                Console.WriteLine(message);
            }
            LogServiceErrorNoCallStack(ServiceName,message);
        }

        public static void LogServiceErrorNoCallStack(string serviceName, string message)
        {
            var userName = String.Empty;
            var environment = String.Empty;
            try
            {
                userName = _GetWindowsUserName();
                environment = SMSClient.Handler.TamEnvironment;
            }
            catch
            {
            }
            TamMaestroEventSource.Log.ServiceErrorNoCallStack(serviceName, message, userName, environment);
        }

        public void LogServiceEvent(string message)
        {
            if (IsConsole)
            {
                message = string.Format("Service Event {0} :: {1}", ServiceName, message);
                Console.WriteLine(message);
            }
            LogServiceEvent(ServiceName,message);
        }

        public static void LogServiceEvent(string serviceName, string message)
        {
            var userName = String.Empty;
            var environment = String.Empty;
            try
            {
                userName = _GetWindowsUserName();
                environment = SMSClient.Handler.TamEnvironment;
            }
            catch
            {
            }

            TamMaestroEventSource.Log.ServiceEvent(serviceName, message, userName, environment);
        }

        private static string _GetWindowsUserName()
        {
            var userName = String.Empty;
            if (ServiceSecurityContext.Current != null)
            {
                userName = ServiceSecurityContext.Current.WindowsIdentity.Name;
            }
            return userName;
        }

        public ServiceStatus GetStatus()
        {
            return (ServiceStatus.Open);
        }

        public abstract void Start();
        public abstract void Stop();

        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        public static ObservableEventListener GetNewErrorListener()
        {
            var listener = new ObservableEventListener();
            listener.LogToRollingFlatFile("error_log.txt", 42342343, "yyyy-MM-dd", RollFileExistsBehavior.Increment, RollInterval.Day);
            listener.EnableEvents(TamMaestroEventSource.Log, EventLevel.Error,Keywords.All);
            return listener;
        }

    }
}
