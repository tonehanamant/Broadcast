using System;
using System.Configuration;
using Tam.Maestro.Services.ContractInterfaces;

namespace Tam.Maestro.Services.Clients
{
    public interface IAppSettings
    {
        TAMEnvironment Environment { get; }
        string SmsUri { get; }
    }

    public class AppSettings : IAppSettings
    {
        public TAMEnvironment Environment
        {
            get
            {
                string lEnvironment = ConfigurationManager.AppSettings["TAMEnvironment"];
                if (string.IsNullOrEmpty(lEnvironment))
                    throw new ApplicationException("Missing configuration for appSetting \"TAMEnvironment\".");

                lEnvironment = lEnvironment.ToUpper().Trim();
                switch (lEnvironment)
                {
                    case ("PROD"):
                        return TAMEnvironment.PROD;
                    case ("QA"):
                        return TAMEnvironment.QA;
                    case ("UAT"):
                        return TAMEnvironment.UAT;
                    case ("LOCAL"):
                        return TAMEnvironment.LOCAL;
                    case ("DEV"):
                        return TAMEnvironment.DEV;
                    default:
                        throw new ApplicationException(string.Format("Invalid configuration for appSetting \"TAMEnvironment\" = %s.", lEnvironment));
                }
            }
        }
        public string SmsUri
        {
            get
            {
                string lSmsPort = ConfigurationManager.AppSettings["SMS_Port"];
                string lSmsHost = ConfigurationManager.AppSettings["SMS_Host"];
                string lSmsName = ConfigurationManager.AppSettings["SMS_Name"];

                if (string.IsNullOrEmpty(lSmsPort))
                    throw new ApplicationException("Missing configuration for appSetting \"SMS_Port\".");
                if (string.IsNullOrEmpty(lSmsHost))
                    throw new ApplicationException("Missing configuration for appSetting \"SMS_Host\".");
                if (string.IsNullOrEmpty(lSmsName))
                    throw new ApplicationException("Missing configuration for appSetting \"SMS_Name\".");

                string lUri = string.Format("%s:%s/%s", lSmsHost, lSmsPort, lSmsName);
                return lUri;
            }
        }
    }
}
