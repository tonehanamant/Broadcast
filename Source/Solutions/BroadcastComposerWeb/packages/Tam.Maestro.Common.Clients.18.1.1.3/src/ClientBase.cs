using System;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.Helpers;

namespace Tam.Maestro.Services.Clients
{
    public abstract class ClientBase<T, U> : IDisposable, IAppSettings
        where T:ITAMService
        where U:class, new()
    {
        protected static object _ProxyLock = new object();
        private static object _SyncLock = new object();

        private static volatile U _Handler = null;

        protected Proxy<T> _Proxy = null;

        private bool disposed = false;

        public static U Handler
        {
            
            get
            {
                try
                {
                    lock (_SyncLock)
                    {
                        if (_Handler == null)
                        {
                            _Handler = new U();
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw GetValidException(ex);
                }
                return _Handler;
            }
        }

        private static Exception GetValidException(Exception ex)
        {
            if (ex != null && ex is System.Reflection.TargetInvocationException && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex;
        }

        protected abstract Proxy<T> CreateProxy();

        protected ClientBase()
        {
            this._Proxy = CreateProxy();
        }
        ~ClientBase()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ServiceStatus GetStatus()
        {
            ServiceStatus lReturn = ServiceStatus.Closed;

            if (this._Proxy != null)
                lReturn = this._Proxy.GetStatus();

            if (lReturn == ServiceStatus.Closed)
            {
                // try reconnecting to the service
                try
                {
                    lock (_ProxyLock)
                    {
                        try
                        {
                            if (this._Proxy != null)
                                this._Proxy.Dispose();
                        }
                        catch { }

                        this._Proxy = null;
                        this._Proxy = CreateProxy();
                    }
                    lReturn = this._Proxy.GetStatus();
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

            lReturn = this._Proxy.RemoteContract.GetMaestroServiceRuntime();
            return lReturn;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    this._Proxy.Dispose();
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        protected void _CheckConnection()
        {
            if (this.GetStatus() != ServiceStatus.Open)
            {
                throw new Exception("Could not open Connection.");
            }
        }

        protected bool _ConnectionNotOpen()
        {
            return GetStatus() != ServiceStatus.Open;
        }

        protected bool _CheckConnection<TC>(TAMResult2<TC> lReturn)
        {
            if (this.GetStatus() != ServiceStatus.Open)
            {
                lReturn.Status = ResultStatus.Error;
                lReturn.Message = _GetServiceUnavailableMessage();
                return true;
            }
            return false;
        }

        protected bool _CheckConnection<TC,TD>(TAMResult2<TC,TD> lReturn)
        {
            if (this.GetStatus() != ServiceStatus.Open)
            {
                lReturn.Status = ResultStatus.Error;
                lReturn.Message = _GetServiceUnavailableMessage();
                return true;
            }
            return false;
        }

        protected bool _CheckConnection(TAMResult lReturn)
        {
            if (this.GetStatus() != ServiceStatus.Open)
            {
                lReturn.Type = ResultStatus.Error;
                lReturn.Message = _GetServiceUnavailableMessage();
                return true;
            }
            return false;
        }

        protected abstract string _GetServiceUnavailableMessage();

        protected T _CheckForExceptionStatusAndThrow<T>(TAMResult2<T> tamResult)
        {
            if (tamResult.Status == ResultStatus.Error)
            {
                if (tamResult.ExceptionType == ExceptionType.ValidationException)
                {
                    throw new TamValidationException(tamResult.Message, tamResult.ExceptionData);
                }
                throw new Exception(tamResult.Message);
            }
            return tamResult.Result;
        }

        protected T _UnpackAndCheckForExceptionStatusAndThrow<T>(TAMResult2<byte[]> tamResult) where T:class 
        {
            if (tamResult.Status == ResultStatus.Error)
            {
                if (tamResult.ExceptionType == ExceptionType.ValidationException)
                {
                    throw new TamValidationException(tamResult.Message, tamResult.ExceptionData);
                }
                throw new Exception(tamResult.Message);
            }
            return tamResult.Result.FromBytes<T>();
        }

        #region IAppSettings
        public TAMEnvironment Environment
        {
            get { return new AppSettings().Environment; }
        }
        public string SmsUri
        {
            get { return new AppSettings().SmsUri; }
        }
        #endregion
    }
}
