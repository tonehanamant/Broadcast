using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Management.Automation;
using System.ServiceModel;
using Common.Systems.DataTransferObjects;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.DatabaseDtos;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Tam.Maestro.Services.Clients
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class SMSClient : IServiceManagerCallback, IDisposable, ISMSClient
    {
        private static volatile SMSClient _SMSClient = null;
        private static object _SyncLock = new object();
        private static object _ProxyLock = new object();
        private Proxy<IServiceManagerContract> _SMSProxy;
        private bool disposed = false;
        private Dictionary<TAMService, string> _UriConfigurationTable;
        private Dictionary<TAMResource, string> _ResourceConfigurationTable;
        private SynchronizedCollection<GenericEvent<TAMService, string>> _uriChangedDelegates = new SynchronizedCollection<GenericEvent<TAMService, string>>();
        private SynchronizedCollection<GenericEvent<TAMResource, string>> _resourceChangedDelegates = new SynchronizedCollection<GenericEvent<TAMResource, string>>();
        private event GenericEvent<TAMService, string> UriChangedEvt = DummyUriChangedEventHandler;  //why?  Is this needed?  I think it may be because we don't want the GC to dispose of SMSClient automatically
        private event GenericEvent<TAMResource, string> ResourceChangedEvt = DummyResourceChangedHandler;

        public TAMEnvironment TamEnvironment { get; private set; }

        public static SMSClient Handler
        {
            get
            {
                lock (_SyncLock)
                {
                    if (_SMSClient == null)
                    {
                        _SMSClient = new SMSClient();
                    }
                }
                return _SMSClient;
            }
        }

        private SMSClient()
        {
            this._UriConfigurationTable = new Dictionary<TAMService, string>();
            this._ResourceConfigurationTable = new Dictionary<TAMResource, string>();
            _uriChangedDelegates.Add(DummyUriChangedEventHandler);
            _resourceChangedDelegates.Add(DummyResourceChangedHandler);

            var tamEnvironment = ConfigurationManager.AppSettings["TAMEnvironment"];
            this.TamEnvironment = SMSHelper.FromStringToTAMEnvironment(tamEnvironment);
        }
        ~SMSClient()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    foreach (GenericEvent<TAMService, string> handler in _uriChangedDelegates)
                        this.UriChangedEvt -= handler;
                    _uriChangedDelegates.Clear();
                    foreach (GenericEvent<TAMResource, string> handler in _resourceChangedDelegates)
                        this.ResourceChangedEvt -= handler;
                    _resourceChangedDelegates.Clear();
                    this._SMSProxy.Dispose();
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        public ServiceStatus GetStatus()
        {
            ServiceStatus lReturn = this._SMSProxy.GetStatus();
            if (lReturn == ServiceStatus.Closed)
            {
                // try reconnecting to the service
                try
                {
                    this._SMSProxy = null;
                    this.Init();
                    lReturn = this._SMSProxy.GetStatus();
                }
                catch
                {
                    lReturn = ServiceStatus.Closed;
                }
            }
            return (lReturn);
        }

        public Tam.Maestro.Services.ContractInterfaces.Common.MaestroServiceRuntime GetMaestroServiceRuntime()
        {
            Tam.Maestro.Services.ContractInterfaces.Common.MaestroServiceRuntime lReturn;
            #region Check Connection
            if (this.GetStatus() != ServiceStatus.Open)
                return null;
            #endregion

            lReturn = this._SMSProxy.RemoteContract.GetMaestroServiceRuntime();
            return lReturn;

        }

        public static void DummyUriChangedEventHandler(TAMService serviceEnum, string item)
        { }
        public static void DummyResourceChangedHandler(TAMResource resource, string item)
        { }
        //so when we call dispose explicity, the GC can do its job
        public event GenericEvent<TAMService, string> UriChanged
        {
            add
            {
                this.UriChangedEvt += value;
                _uriChangedDelegates.Add(value);
            }
            remove
            {
                this.UriChangedEvt -= value;
                _uriChangedDelegates.Remove(value);
            }
        }
        public event GenericEvent<TAMResource, string> ResourceChanged
        {
            add
            {
                this.ResourceChangedEvt += value;
                _resourceChangedDelegates.Add(value);
            }
            remove
            {
                this.ResourceChangedEvt -= value;
                _resourceChangedDelegates.Remove(value);
            }
        }

        private void Init()
        {
            if (this._SMSProxy != null)
            {
                return;
            }
            CheckAndRecreateSMSProxy();
        }

        bool _proxyCreationInprogress = false;
        private void CheckAndRecreateSMSProxy()
        {
            if (!_proxyCreationInprogress && this._SMSProxy != null && 
                this._SMSProxy.GetStatus() == ServiceStatus.Open)
            {
                return;
            }

            lock (_ProxyLock)
            {
                if (this._SMSProxy != null && this._SMSProxy.GetStatus() == ServiceStatus.Open)
                {
                    return;
                }

                try
                {
                    _proxyCreationInprogress = true;
                    this._SMSProxy = null;

                    string lUri = "net.tcp://" +  ConfigurationManager.AppSettings["SMS_Host"] + ":" +  
                        ConfigurationManager.AppSettings["SMS_Port"] + "/" +   ConfigurationManager.AppSettings["SMS_Name"];
                    this._SMSProxy = new Proxy<IServiceManagerContract>(TAMService.ServiceManagerService, this, lUri);

                    if (this._SMSProxy.GetStatus() == ServiceStatus.Open)
                    {
                        this._SMSProxy.RemoteContract.Subscribe();
                        this.LoadResources();
                    }
                    else
                    {
                        throw new Exception("Not able to connect to SMS service.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw new Exception("Not able to connect to SMS service.", ex);
                }
                finally
                {
                    _proxyCreationInprogress = false;
                }
            }
        }

        private void LoadResources()
        {
            foreach (TAMResource lTAMResource in Enum.GetValues(typeof(TAMResource)))
            {
                this._ResourceConfigurationTable[lTAMResource] = this._SMSProxy.RemoteContract.GetResource(this.TamEnvironment, lTAMResource).Result;
            }
        }
        private string ResolveUri(TAMService tamService)
        {
            string uri = "";
            if (this._UriConfigurationTable.ContainsKey(tamService))
            {
                uri = this._UriConfigurationTable[tamService].ToString();
            }
            else
            {
                CheckAndRecreateSMSProxy();
                TAMResult2<string> result = this._SMSProxy.RemoteContract.GetUri(this.TamEnvironment, tamService);
                if (result.Status == ResultStatus.Error)
                {
                    throw new ApplicationException(result.Message);
                }
                uri = result.Result;
            }
            return uri;
        }

        public Proxy<T> CreateProxy<T>(TAMService tamService, object pCallbackObject) where T : ITAMService
        {
            try
            {
                this.Init();
                Proxy<T> lProxy = new Proxy<T>(tamService, pCallbackObject, ResolveUri(tamService));
                return lProxy;
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
                throw;
            }
        }
        public TamProxy<T> CreateTamProxy<T>(TAMService tamService, object pCallbackObject, string uri = null) where T : ITAMService
        {
            if (string.IsNullOrWhiteSpace(uri))
                uri = ResolveUri(tamService);
            try
            {
                this.Init();
                TamProxy<T> lProxy = new TamProxy<T>(tamService, pCallbackObject, uri);

                return lProxy;
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
                throw;
            }
        }
        public string GetUri<T>(TAMService lTamService) where T : ITAMService
        {
            try
            {
                this.Init();
                string lUri = "";
                if (this._UriConfigurationTable.ContainsKey(lTamService))
                {
                    lUri = this._UriConfigurationTable[lTamService].ToString();
                }
                else
                {
                    CheckAndRecreateSMSProxy();
                    TAMResult2<string> lResult = this._SMSProxy.RemoteContract.GetUri(this.TamEnvironment, lTamService);
                    if (lResult.Status == ResultStatus.Error)
                    {
                        throw new Exception(lResult.Message);
                    }
                    lUri = lResult.Result;
                }
                return lUri;
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
                throw;
            }
        }

        public SmsDbConnectionInfo GetSmsDbConnectionInfo(TAMResource pTamResource)
        {
            SmsDbConnectionInfo lReturn = new SmsDbConnectionInfo(pTamResource, "", "");
            try
            {
                System.Data.SqlClient.SqlConnectionStringBuilder lBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(this.GetResource(pTamResource));
                lReturn = new SmsDbConnectionInfo(pTamResource, lBuilder.DataSource, lBuilder.InitialCatalog);
            }
            catch { }
            return lReturn;
        }
        public string GetResource(TAMResource pTamResource)
        {
            try
            {
                this.Init();
                string lResource = "";
                if (this._ResourceConfigurationTable.ContainsKey(pTamResource))
                {
                    lResource = this._ResourceConfigurationTable[pTamResource].ToString();
                }
                else
                {
                    CheckAndRecreateSMSProxy();
                    TAMResult2<string> lResult = this._SMSProxy.RemoteContract.GetResource(this.TamEnvironment, pTamResource);
                    if (lResult.Status == ResultStatus.Error)
                    {
                        throw new Exception(lResult.Message);
                    }
                    lResource = lResult.Result;
                }
                return lResource;
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
                throw;
            }
        }

        public string GetSystemComponentParameterValue(string pSystemComponentID, string pSystemParameterID)
        {
            try
            {
                CheckAndRecreateSMSProxy();
                TAMResult2<string> lResult = this._SMSProxy.RemoteContract.GetSystemComponentParameterValue(pSystemComponentID, pSystemParameterID);
                if (lResult.Status == ResultStatus.Error)
                {
                    throw new Exception(lResult.Message);
                }
                return lResult.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public bool ClearSystemComponentParameterCache(string pSystemComponentID, string pSystemParameterID)
        {
            try
            {
                CheckAndRecreateSMSProxy();
                TAMResult2<bool> lResult = this._SMSProxy.RemoteContract.ClearSystemComponentParameterCache(pSystemComponentID, pSystemParameterID);
                if (lResult.Status == ResultStatus.Error)
                {
                    throw new Exception(lResult.Message);
                }
                return lResult.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public TAMResult2<string> TestConnection(TAMResource pTamResource)
        {
            TAMResult2<string> lReturn = new TAMResult2<string>();
            try
            {
                CheckAndRecreateSMSProxy();
                lReturn = this._SMSProxy.RemoteContract.TestConnection(this.TamEnvironment, pTamResource);
            }
            catch (Exception exc)
            {
                lReturn.Message = exc.Message;
                lReturn.Status = ResultStatus.Error;
            }
            return lReturn;
        }

        public void OnUriChanged(ServiceManagerServiceEventArgs pArgs)
        {
            if (this.TamEnvironment == pArgs.EnvironmentChanged)
            {
                lock (_SyncLock)
                {
                    this._UriConfigurationTable[pArgs.ServiceChanged] = pArgs.NewUri;
                }
                Delegate[] lDelegates = this.UriChangedEvt.GetInvocationList();
                foreach (Delegate lDelegate in lDelegates)
                {
                    ((GenericEvent<TAMService, string>)lDelegate).BeginInvoke(pArgs.ServiceChanged, pArgs.NewUri, null, null);
                }
            }
        }
        public void OnResourceChanged(ServiceManagerResourceEventArgs pArgs)
        {
            if (this.TamEnvironment == pArgs.EnvironmentChanged)
            {
                lock (_SyncLock)
                {
                    this._ResourceConfigurationTable[pArgs.ResourceChanged] = pArgs.NewValue;
                }
                Delegate[] lDelegates = this.ResourceChangedEvt.GetInvocationList();
                foreach (Delegate lDelegate in lDelegates)
                {
                    ((GenericEvent<TAMResource, string>)lDelegate).BeginInvoke(pArgs.ResourceChanged, pArgs.NewValue, null, null);
                }
            }
        }

        public bool ValidateEnvironment()
        {
            try
            {
                var databaseEnvironment = GetSystemComponentParameterValue("MaestroEnvironment", "Environment");
                var appSettingsEnvironment = SMSClient.Handler.TamEnvironment.Description();

                if (!appSettingsEnvironment.Contains(databaseEnvironment))
                {
                    Console.WriteLine("Unable to start service stack. Environment configuration mismatch...\nDatabase: {0}\nAppSettings: {1}", databaseEnvironment, appSettingsEnvironment);
                    using (PowerShell PowerShellInstance = PowerShell.Create())
                    {
                        PowerShellInstance.AddScript("(gwmi win32_process | where {$_.commandline -match \"STARTUP.BAT\"} | select processID).processID");

                        // invoke execution on the pipeline (collecting output)
                        Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                        // loop through each output object item
                        foreach (PSObject outputItem in PSOutput)
                        {
                            // if null object was dumped to the pipeline during the script then a null
                            // object may be present here. check for null to prevent potential NRE.
                            if (outputItem != null)
                            {
                                Process pToKill = Process.GetProcessById(int.Parse(outputItem.BaseObject.ToString()));
                                pToKill.Kill();
                            }
                        }
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public TAMResult IsLocked(IBusinessEntity[] pEntities)
        {
            TAMResult lReturn = new TAMResult();
            try
            {
                CheckAndRecreateSMSProxy();
                lReturn = this._SMSProxy.RemoteContract.IsLockedBin(pEntities.ToByteArray());
            }
            catch (Exception exc)
            {
                lReturn.Type = ResultStatus.Error;
                lReturn.Message = exc.Message;
            }
            return (lReturn);
        }

        public TAMResult LockEntity(IBusinessEntity[] pEntities)
        {
            TAMResult lReturn = new TAMResult();
            try
            {
                CheckAndRecreateSMSProxy();
                lReturn = this._SMSProxy.RemoteContract.LockEntityBin(pEntities.ToByteArray());
            }
            catch (Exception exc)
            {
                lReturn.Type = ResultStatus.Error;
                lReturn.Message = exc.Message;
            }
            return (lReturn);
        }

        public void ReleaseEntity(IBusinessEntity[] pEntities)
        {
            try
            {
                CheckAndRecreateSMSProxy();
                this._SMSProxy.RemoteContract.ReleaseEntityBin(pEntities.ToByteArray());
            }
            catch { }
        }

        //BEGIN Locking around the ui
        public LockResponse LockObject(string key, string userId)
        {
            CheckAndRecreateSMSProxy();
            return this._SMSProxy.RemoteContract.LockObject(key, userId);
        }

        public ReleaseLockResponse ReleaseObject(string key, string userId)
        {
            CheckAndRecreateSMSProxy();
            return this._SMSProxy.RemoteContract.ReleaseObject(key, userId);
        }

        public bool IsObjectLocked(string key, string userId)
        {
            CheckAndRecreateSMSProxy();
            return this._SMSProxy.RemoteContract.IsObjectLocked(key, userId);
        }
        //END Locking around the ui

        public MaestroImage GetLogoImage(CMWImageEnums logoType)
        {
            CheckAndRecreateSMSProxy();
            var ret = this._SMSProxy.RemoteContract.GetLogoImage(logoType);
            if (ret.Status != ResultStatus.Success)
            {
                throw new Exception(ret.Message);
            }
            return ret.Result;
        }

        public List<LookupDto> GetActiveAdvertisers()
        {
            CheckAndRecreateSMSProxy();
            var ret = this._SMSProxy.RemoteContract.GetActiveAdvertisers();
            if (ret.Status != ResultStatus.Success)
            {
                throw new Exception(ret.Message);
            }
            return ret.Result;
        }

        public List<LookupDto> FindAdvertisersByIds(List<int> advertiserIds)
        {
            CheckAndRecreateSMSProxy();
            var ret = this._SMSProxy.RemoteContract.FindAdvertisersByIds(advertiserIds);
            if (ret.Status != ResultStatus.Success)
            {
                throw new Exception(ret.Message);
            }
            return ret.Result;
        }

        public LookupDto FindAdvertiserById(int advertiserId)
        {
            CheckAndRecreateSMSProxy();
            var ret = this._SMSProxy.RemoteContract.FindAdvertiserById(advertiserId);
            if (ret.Status != ResultStatus.Success)
            {
                throw new Exception(ret.Message);
            }
            return ret.Result;
        }

        public AuthenticationEmployee GetEmployee(string sssid, bool forceReset = false)
        {
            CheckAndRecreateSMSProxy();
            var ret = this._SMSProxy.RemoteContract.GetEmployee(sssid, forceReset);
            if (ret.Status != ResultStatus.Success)
            {
                throw new Exception(ret.Message);
            }
            return ret.Result;
        }
    }
}