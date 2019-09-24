using log4net;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using Tam.Maestro.Common.Utilities.Logging;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces;

namespace Tam.Maestro.Services.Clients
{
    public class Proxy<T> : IDisposable
        where T : ITAMService
    {
        private T _MyContractChannel;
        private T _ChannelChecker;
        private object _Implementation;
        private Uri _Uri;
        private InstanceContext _Context;
        private ChannelFactory<T> _ChannelFactory;
        private TAMService _TAMService;
        private static readonly ILog Log = LogManager.GetLogger(typeof (Proxy<>));
        private const int TIMEOUT = 30;

        public T RemoteContract
        {
            get
            {
                return this._MyContractChannel;
            }
        }

        public Proxy(TAMService tamService, object pImplementation, string pUri)
        {
            this._TAMService = tamService;
            this._Uri = new Uri(pUri);
            this._Implementation = pImplementation;
            this.Init();
        }

        public void Init()
        {
            NetTcpBinding lBinding = new NetTcpBinding(SecurityMode.Transport);
            lBinding.ReceiveTimeout = TimeSpan.MaxValue;
            lBinding.SendTimeout = TimeSpan.MaxValue;
            lBinding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
            lBinding.ReliableSession.Enabled = true;
            lBinding.MaxBufferPoolSize = int.MaxValue;
            lBinding.MaxBufferSize = int.MaxValue;
            lBinding.MaxConnections = 10000;
            lBinding.MaxReceivedMessageSize = int.MaxValue;
            lBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            lBinding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            lBinding.ReaderQuotas.MaxDepth = int.MaxValue;
            lBinding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
            lBinding.ReaderQuotas.MaxStringContentLength = int.MaxValue;

            EndpointAddress lAddress = new EndpointAddress(this._Uri, EndpointIdentity.CreateSpnIdentity(this._Uri.LocalPath.Replace("/", "") + "/" + this._Uri.DnsSafeHost));
            this._Context = new InstanceContext(this._Implementation);
            this._ChannelFactory = this._Implementation == null ?
                this._ChannelFactory = new ChannelFactory<T>(lBinding, lAddress) :
                this._ChannelFactory = new DuplexChannelFactory<T>(this._Context, lBinding, lAddress);
            foreach (OperationDescription lOperation in this._ChannelFactory.Endpoint.Contract.Operations)
                lOperation.Behaviors.Find<DataContractSerializerOperationBehavior>().MaxItemsInObjectGraph = int.MaxValue;
            this._MyContractChannel = this._ChannelFactory.CreateChannel();

            lBinding.SendTimeout = TimeSpan.FromSeconds(10);
            this._ChannelFactory = this._Implementation == null ?
                this._ChannelFactory = new ChannelFactory<T>(lBinding, lAddress) :
                this._ChannelFactory = new DuplexChannelFactory<T>(this._Context, lBinding, lAddress);
            foreach (OperationDescription lOperation in this._ChannelFactory.Endpoint.Contract.Operations)
                lOperation.Behaviors.Find<DataContractSerializerOperationBehavior>().MaxItemsInObjectGraph = int.MaxValue;
            this._ChannelChecker = this._ChannelFactory.CreateChannel();
        }

        public ServiceStatus GetStatus()
        {
            ServiceStatus lReturn = ServiceStatus.Closed;
            try
            {
                if (this.State == CommunicationState.Faulted)
                {
                    this._ChannelFactory.Abort();
                }
                else
                {
                    try
                    {
                        lReturn = this._ChannelChecker.GetStatus();
                    }
                    catch(Exception e)
                    {
                        LogHelper.Log.ServiceError("Proxy", e.Message, e.ToString(), "", "");
                        this._ChannelFactory.Abort();
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log.ServiceError("Proxy", e.Message, e.ToString(), "", "");
                lReturn = ServiceStatus.Closed;
            }
            return lReturn;
        }

        public void Dispose()
        {
            try
            {
                CloseOrAbortClientChannel((ICommunicationObject)this._ChannelChecker);
                ((IDisposable) this._ChannelChecker).Dispose();
                this._ChannelChecker = default(T);
            }
            catch (Exception ex)
            {
                LogHelper.Log.ServiceError("Proxy", ex.Message, ex.ToString(), "", "");
                Log.Warn("Error disposing _ChannelChecker", ex);
            }

            try
            {
                CloseOrAbortClientChannel((ICommunicationObject)this._MyContractChannel);
                ((IDisposable) this._MyContractChannel).Dispose();
                this._MyContractChannel = default(T);
            }
            catch (Exception ex2)
            {
                LogHelper.Log.ServiceError("Proxy", ex2.Message, ex2.ToString(), "", "");
                Log.Warn("Error disposing _MyContractChannel.", ex2);
            }

            try
            {
                CloseOrAbortClientChannel(this._Context);
            }
            catch (Exception ex3)
            {
                LogHelper.Log.ServiceError("Proxy", ex3.Message, ex3.ToString(), "", "");
                Log.Warn("Error closing _Context", ex3);
            }

            try
            {
                CloseOrAbortClientChannel(this._ChannelFactory);
                this._ChannelFactory = null;
            }
            catch (Exception ex4)
            {
                LogHelper.Log.ServiceError("Proxy", ex4.Message, ex4.ToString(), "", "");
                Log.Warn("Error closing _ChannelFactory.", ex4);
            }
        }

        public void CloseOrAbortClientChannel(ICommunicationObject clientChannel)
        {
            try
            {
                if (clientChannel == null)
                    return;

                if (clientChannel.State == CommunicationState.Faulted)
                {
                    clientChannel.Abort();
                }
                else
                {
                    clientChannel.Close();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.ServiceError("Proxy", ex.Message, ex.ToString(), "", "");
                Log.Warn("Error closing channel.", ex);
            }
        }

        public void OutputStats()
        {
            StringBuilder lBuilder = new StringBuilder();
            lBuilder.Append("Incoming Channels: " + this._Context.IncomingChannels.Count + "\n");
            lBuilder.Append("Outgoing Channels: " + this._Context.OutgoingChannels.Count + "\n");
            lBuilder.Append("ManualFlowControlLimit: " + this._Context.ManualFlowControlLimit + "\n");
            System.Diagnostics.Debug.WriteLine(lBuilder.ToString());
        }

        /// <summary>
        /// The state of the WCF channel communicating with the service.
        /// </summary>
        /// <returns>
        /// CommunicationState.Faulted if the channel is busted.
        /// </returns>
        public CommunicationState State
        {
            get
            {
                return ((IClientChannel)this._MyContractChannel).State;
            }
        }
        /// <summary>
        /// Gets or sets the time period within which an operation must complete or an
        /// exception is thrown.
        /// Use this so that the app won't hang on "loading" status rectangle if something is taking too long.
        /// </summary>
        /// <returns>
        /// A TimeSpan that specifies the time period within which an operation must complete.
        /// </returns>
        public TimeSpan OperationTimeout
        {
            get
            {
                return ((IClientChannel)this._MyContractChannel).OperationTimeout;
            }
        }

        /// <summary>
        /// Occurs when the communication object first enters the faulted state.
        /// Attach to this event in your Forms or even in the Main method.
        /// </summary>
        public event EventHandler Faulted
        {
            add { ((IClientChannel)this._MyContractChannel).Faulted += value; }
            remove { ((IClientChannel)this._MyContractChannel).Faulted -= value; }
        } 


    }
}