using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using Tam.Maestro.Common.Logging;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Services
{
    public abstract class ServiceBase
    {
        private static string _serviceName;

        protected ServiceBase(string serviceName)
        {
            _serviceName = serviceName;
        }

        protected void TimeResultsNoReturn(String methodName, Action func)
        {
            var userName = _GetWindowsUserName();
            var environment = SMSClient.Handler.TamEnvironment;

            TamMaestroEventSource.Log.ServiceCallStart(GetServiceName(), methodName, userName, environment);

            var stopWatch = Stopwatch.StartNew();
            func();
            stopWatch.Stop();

            TamMaestroEventSource.Log.ServiceCallStop(GetServiceName(), methodName, userName, environment);
            TamMaestroEventSource.Log.ServiceCallTotalTime(GetServiceName(), methodName, stopWatch.Elapsed.TotalSeconds, userName, environment);
        }

        protected T TimeResults<T>(String methodName, Func<T> func)
        {
            var userName = _GetWindowsUserName();
            var environment = SMSClient.Handler.TamEnvironment;

            TamMaestroEventSource.Log.ServiceCallStart(GetServiceName(), methodName, userName, environment);

            var stopWatch = Stopwatch.StartNew();
            var results = func();
            stopWatch.Stop();

            TamMaestroEventSource.Log.ServiceCallStop(GetServiceName(), methodName, userName, environment);
            TamMaestroEventSource.Log.ServiceCallTotalTime(GetServiceName(), methodName, stopWatch.Elapsed.TotalSeconds, userName, environment);

            return results;
        }

        public void LogServiceError(string message, Exception exception)
        {
            LogServiceError(GetServiceName(),message,exception);
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
            LogServiceErrorNoCallStack(GetServiceName(),message);
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
            LogServiceEvent(GetServiceName(),message);
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

        protected abstract string GetServiceName();

        public static string GetServiceNameStaticPlaceholder()
        {
            return _serviceName;
        }

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

    }
}
