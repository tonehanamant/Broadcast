using System;
using System.Configuration;
using System.Data.Entity;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using ConfigurationService.Client;
using Services.Broadcast;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Common.Services.Repositories
{
    public class RepositoryBase<CT> : CoreRepositoryBase<CT> where CT : DbContext
    {
        private int _Timeout;
        private IContextFactory<CT> _ContextFactory;
        private ISMSClient _SmsClient;
        private IConfigurationWebApiClient _ConfigurationWebApiClient;
        private readonly string _ConnectionStringType;
        private readonly string _Resource;
        MemoryCache _Cache = MemoryCache.Default;

        public RepositoryBase(ISMSClient pSmsClient, IContextFactory<CT> pContextFactory, 
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient, string connectionStringType)
            : base(pSmsClient, pContextFactory, pTransactionHelper, connectionStringType)
        {
            _SmsClient = pSmsClient;
            _ConfigurationWebApiClient = configurationWebApiClient;
            _ContextFactory = pContextFactory;
            _ConnectionStringType = connectionStringType;

            string appSetting = ConfigurationManager.AppSettings["EntityFrameworkCommandTimeoutInSeconds"];
            _Timeout = appSetting != null ? int.Parse(appSetting) : 300;
        }

        protected override CT CreateDBContext(bool pOptimizeForBulkIsert)
        {
            string connectionString = GetConnectionString();
            CT ct = _ContextFactory.FromTamConnectionString(connectionString);
            if ((object)ct != null)
            {
                ct.Database.CommandTimeout = _Timeout;
                if (pOptimizeForBulkIsert)
                {
                    ct.Configuration.ValidateOnSaveEnabled = false;
                    ct.Configuration.AutoDetectChangesEnabled = false;
                }
            }
            return ct;
        }

        private bool ShouldUseSMSClient()
        {
            bool useSMS;
            var hasConfig = bool.TryParse(ConfigurationManager.AppSettings["ConfigurationClientUseSMS"], out useSMS);
            return hasConfig ? useSMS : true;
        }

        private string GetConnectionString()
        {
            if (_Cache.Contains(_ConnectionStringType))
            {
                return _Cache[_ConnectionStringType] as string;
            }

            string resource = _ConnectionStringType == TAMResource.BroadcastConnectionString.ToString()
                ? "broadcast"
                : "broadcastforecast";

            string connectionString = ShouldUseSMSClient() 
                ? _SmsClient.GetResource(_ConnectionStringType)
                : _ConfigurationWebApiClient.GetResource(resource);

            _Cache.Set(_ConnectionStringType, connectionString, new CacheItemPolicy()
                { AbsoluteExpiration = DateTime.UtcNow.AddSeconds(30) });
            return connectionString;
        }
    }
}