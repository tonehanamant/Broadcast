using System;
using Common.Services.Repositories;
using ConfigurationService.Client;
using ConfigurationService.Interfaces.Dtos;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.PowerShell.Commands;
using Services.Broadcast;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Tam.Maestro.Services.Clients
{
    public class ConfigurationClientSwitch : IConfigurationWebApiClient
    {
        private ISMSClient _smsClient;
        private static volatile ConfigurationClientSwitch _handler = (ConfigurationClientSwitch)null;
        private static readonly object SyncLock = new object();

        public ConfigurationClientSwitch(ISMSClient smsClient)
        {
            _smsClient = smsClient;
        }

        public static ConfigurationClientSwitch Handler
        {
            get
            {
                lock (SyncLock)
                {
                    if (_handler == null)
                        _handler = new ConfigurationClientSwitch(SMSClient.Handler);
                }
                return _handler;
            }
        }

        public string TAMEnvironment
        {
            get { return MaestroEnvironmentSystemParameter.Environment; }
        }

        public bool ClearSystemComponentParameterCache(string componentID, string parameterID)
        {
            if (ShouldUseSMSClient())
            {
                return _smsClient.ClearSystemComponentParameterCache(componentID, parameterID);
            }
            else
            {
                return ConfigurationWebApiClient.Handler.ClearSystemComponentParameterCache(componentID, parameterID);
            }
        }

        public bool ClearSystemComponentParameterCache()
        {
            // we need this to get cleared regardless to avoid stale db connection strings
            SMSClient.Handler.ClearSystemComponentParameterCache(null, null);

            if (!ShouldUseSMSClient())
            {
                return ConfigurationWebApiClient.Handler.ClearSystemComponentParameterCache();
            }

            return true;
        }

        public string GetResource(string resource)
        {
            // Note: The SMS Service and the Configuration API take slightly different strings as input
            string databaseName = resource == TAMResource.BroadcastConnectionString.ToString()
                ? "broadcast"
                : "broadcastforecast";

            if (ShouldUseSMSClient())
            {
                return _smsClient.GetResource(resource);
            }
            else
            {
                return ConfigurationWebApiClient.Handler.GetResource(databaseName);
            }
        }

        public SystemComponentParameter GetSystemComponentParameter(string componentId, string parameterId)
        {
            if (ShouldUseSMSClient())
            {
                throw new NotImplementedException();
            }
            else
            {
                return ConfigurationWebApiClient.Handler.GetSystemComponentParameter(componentId, parameterId);
            }
        }

        public string GetSystemComponentParameterValue(string componentId, string parameterId)
        {
            if (ShouldUseSMSClient())
            {
                return _smsClient.GetSystemComponentParameterValue(componentId, parameterId);
            }
            else
            {
                return ConfigurationWebApiClient.Handler.GetSystemComponentParameterValue(componentId, parameterId);
            }
        }

        public void SaveSystemComponentParameters(List<SystemComponentParameter> paramList)
        {
            if (ShouldUseSMSClient())
            {
                throw new NotImplementedException();
            }
            else
            {
                ConfigurationWebApiClient.Handler.SaveSystemComponentParameters(paramList);
            }
        }

        public List<SystemComponentParameter> SearchSystemComponentParameters(string componentId, string parameterId)
        {
            if (ShouldUseSMSClient())
            {
                throw new NotImplementedException();
            }
            else
            {
                return ConfigurationWebApiClient.Handler.SearchSystemComponentParameters(componentId, parameterId);
            }
        }

        private bool ShouldUseSMSClient()
        {
            bool useSMS;
            var hasConfig = bool.TryParse(ConfigurationManager.AppSettings["ConfigurationClientUseSMS"], out useSMS);
            return hasConfig ? useSMS : true;
        }
    }
}
