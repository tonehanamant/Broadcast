using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.ServiceModel;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.Logging;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.Helpers;

namespace Services.Broadcast.Services
{
    public abstract class ServiceBase
    {
        private BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private static string _serviceName;

        protected ServiceBase(string serviceName)
        {
            _serviceName = serviceName;
            _ApplicationServiceFactory = new BroadcastApplicationServiceFactory();
        }

        protected void TimeResultsNoReturn(String methodName, Action func)
        {
            var userName = _GetWindowsUserName();
            var environment = GetDescription(SMSClient.Handler.TamEnvironment);

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
            var environment = GetDescription(SMSClient.Handler.TamEnvironment);

            TamMaestroEventSource.Log.ServiceCallStart(GetServiceName(), methodName, userName, environment);

            var stopWatch = Stopwatch.StartNew();
            var results = func();
            stopWatch.Stop();

            TamMaestroEventSource.Log.ServiceCallStop(GetServiceName(), methodName, userName, environment);
            TamMaestroEventSource.Log.ServiceCallTotalTime(GetServiceName(), methodName, stopWatch.Elapsed.TotalSeconds, userName, environment);

            return results;
        }

        protected TAMResult2<byte[]> _WrapWithTAMResult2AsByteArray<T>(Func<T> codeToRun)
        {
            var lReturn = new TAMResult2<byte[]>();
            try
            {
                lReturn.Result = codeToRun().ToByteArray();
            }
            catch (Exception e)
            {
                lReturn.Status = ResultStatus.Error;
                lReturn.Message = e.Message;
                if (e as TamValidationException != null)
                {
                    var validationException = e as TamValidationException;
                    lReturn.ExceptionType = ExceptionType.ValidationException;
                    lReturn.ExceptionData = validationException.ValidationErrors;
                }
                ServiceBase.LogServiceError(ServiceBase.GetServiceNameStaticPlaceholder(), lReturn.Message, e);
            }
            return lReturn;
        }

        protected TAMResult2<T> _WrapWithTAMResult2<T>(Func<T> codeToRun)
        {
            var lReturn = new TAMResult2<T>();
            try
            {
                lReturn.Result = codeToRun();
            }
            catch (Exception e)
            {
                lReturn.Status = ResultStatus.Error;
                lReturn.Message = e.Message;
                if (e as TamValidationException != null)
                {
                    var validationException = e as TamValidationException;
                    lReturn.ExceptionType = ExceptionType.ValidationException;
                    lReturn.ExceptionData = validationException.ValidationErrors;
                }
                ServiceBase.LogServiceError(ServiceBase.GetServiceNameStaticPlaceholder(), lReturn.Message, e);
            }
            return lReturn;
        }

        public static void LogServiceError(string serviceName, string message, Exception exception)
        {
            var userName = String.Empty;
            var environment = String.Empty;
            try
            {
                userName = _GetWindowsUserName();
                environment = GetDescription(SMSClient.Handler.TamEnvironment);
            }
            catch
            {
            }
            TamMaestroEventSource.Log.ServiceError(serviceName, exception.Message, exception.ToString(), userName, environment);
        }

        public static void LogServiceErrorNoCallStack(string serviceName, string message)
        {
            var userName = String.Empty;
            var environment = String.Empty;
            try
            {
                userName = _GetWindowsUserName();
                environment = GetDescription(SMSClient.Handler.TamEnvironment);
            }
            catch
            {
            }
            TamMaestroEventSource.Log.ServiceErrorNoCallStack(serviceName, message, userName, environment);
        }

        public static void LogServiceEvent(string serviceName, string message)
        {
            var userName = String.Empty;
            var environment = String.Empty;
            try
            {
                userName = _GetWindowsUserName();
                environment = GetDescription(SMSClient.Handler.TamEnvironment);
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
