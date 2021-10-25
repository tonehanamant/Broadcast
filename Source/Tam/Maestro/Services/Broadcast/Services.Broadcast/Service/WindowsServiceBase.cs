using log4net;
using Services.Broadcast.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Services
{
    public abstract class WindowsServiceBase : BroadcastBaseClass
    {
        private readonly ILog _Log;

        protected WindowsServiceBase(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _Log = LogManager.GetLogger(GetType());
        }
        public abstract string ServiceName { get; }
        public bool IsConsole { get; set; }

        public static string TamEnvironment
        {
            // TODO SDE : Is this right?  It's how the environment service is doing it...
            get => new AppSettings().Environment.ToString(); 
        }

        protected void TimeResultsNoReturn(String methodName, Action func)
        {
            var userName = _GetWindowsUserName();

            _LogInfo("Service call starting.", userName, methodName);

            var stopWatch = Stopwatch.StartNew();
            func();
            stopWatch.Stop();

            _LogInfo($"Service call stopping.  Duration (total seconds) : {stopWatch.Elapsed.TotalSeconds}", userName, methodName);
        }


        protected T TimeResults<T>(String methodName, Func<T> func)
        {
            var userName = _GetWindowsUserName();
            var environment = TamEnvironment;

            _LogInfo("Service call starting.", userName, methodName);

            var stopWatch = Stopwatch.StartNew();
            var results = func();
            stopWatch.Stop();

            _LogInfo($"Service call stopping.  Duration (total seconds) : {stopWatch.Elapsed.TotalSeconds}", userName, methodName);

            return results;
        }

        public void LogServiceError(string message, Exception exception = null)
        {
            if (IsConsole)
            {
                message = string.Format("Service Error {0} :: {1}\\n{2}", ServiceName, message,exception);
                Console.WriteLine(message);
            }
            _LogError(message, exception, ServiceName);
        }

        public void LogServiceEvent(string message)
        {
            if (IsConsole)
            {
                message = string.Format("Service Event {0} :: {1}", ServiceName, message);
                Console.WriteLine(message);
            }
            _LogError(message, null, ServiceName);
        }

        private static string _GetWindowsUserName()
        {
            var userName = String.Empty;
            try
            {
                if (ServiceSecurityContext.Current != null)
                {
                    userName = ServiceSecurityContext.Current.WindowsIdentity.Name;
                }
            }
            catch
            {
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
    }
}
