using log4net;
using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces;

namespace Tam.Maestro.Services.Clients
{
    //this class isn't really a proxy, but neither is Proxy.cs.  Just following the established naming convention.  A true proxy would wrap each service method (much like the XXXClients).
    public class TamProxy<T> : IDisposable
        where T : ITAMService
    {
        //It is the job of the SMSClient to create a TamProxy.  Because of the nature of the constructor used in SMSClient - it doesn't use a config file to new it up - a TamProxy<T> will automatically be opened upon newing it up.
        private const int DEFAULT_CHANNEL_CHECKER_TIMEOUT = 60; //SECONDS
        private T _channel;
        private readonly object _implementation;
        private Uri _uri;
        private InstanceContext _instanceContext;
        private readonly TAMService _tamSerivice;
        private static object _syncLock = new object();
        private static readonly ILog log = LogManager.GetLogger(typeof(TamProxy<>));

        public event EventHandler<EventArgs> ContextOpened;
        public event EventHandler<EventArgs> ContextClosed;
        public event EventHandler<EventArgs> ContextFaulted;
        public event EventHandler<EventArgs> ContextAborted;
        public event EventHandler<EventArgs> ChannelOpening;
        public event EventHandler<EventArgs> ChannelOpened;
        public event EventHandler<EventArgs> ChannelClosed;
        public event EventHandler<EventArgs> ChannelClosing;
        public event EventHandler<EventArgs> ChannelFaulted;

        /// <summary>
        /// All calls, whether made through XXXClient.Handler calls or through the XXXClient.Handler.Contract calls go through this property.  Food for thought...  
        /// At this point, I don't see a reason to occupy it with more than a simple get
        /// </summary>
        public T RemoteContract
        {
            get
            {
                return _channel;
            }
        }

        /// <summary>
        /// This constructor is needed for passing in an InstanceContext (pImplementation) and a Uri to connect to (pUri).
        /// </summary>
        /// <param name="pImplementation">A TamClient instance.</param>
        /// <param name="pUri">Used in QA and production by an appsettings.xml url and port.  Used by test client by passing in (hardcoding) an arbitrary port like 500001 - see the ClientIntegrationTest of MaestroComplete.</param>
        internal TamProxy(TAMService tamService, object pImplementation, string pUri)
        {
            _tamSerivice = tamService;
            _uri = new Uri(pUri);
            _implementation = pImplementation;
            Init();
        }

        private int GetStatusTimeout
        {
            get
            {
                try
                {
                    var setting = ConfigurationManager.AppSettings["GetStatusTimeout"];
                    if (string.IsNullOrWhiteSpace(setting)) return DEFAULT_CHANNEL_CHECKER_TIMEOUT;
                    int timeout;
                    return int.TryParse(setting, out timeout) ? timeout : DEFAULT_CHANNEL_CHECKER_TIMEOUT;
                }
                catch (Exception)
                {
                    return DEFAULT_CHANNEL_CHECKER_TIMEOUT;
                }
            }
        }


        /// <summary>
        /// Create the binding - NetTcpBinding.  And the InstanceContext - some arbitrary XXXClient.  Also sets up the default props needed for Maesto to do its thing.
        /// </summary>
        private void Init()
        {
            lock (_syncLock)
            {
                NetTcpBinding binding = CreateBinding(TimeSpan.MaxValue);
                var address = new EndpointAddress(_uri, EndpointIdentity.CreateSpnIdentity(_uri.LocalPath.Replace("/", "") + "/" + _uri.DnsSafeHost));

                _instanceContext = new InstanceContext(_implementation);
                _instanceContext.Closed += OnContextClosed;
                _instanceContext.Faulted += OnContextFaulted;
                _instanceContext.Opened += OnContextOpened;
                

                ChannelFactory<T> channelFactory = _implementation == null ?
                    new ChannelFactory<T>(binding, address) :
                    new DuplexChannelFactory<T>(_instanceContext, binding, address);

                foreach (OperationDescription operation in channelFactory.Endpoint.Contract.Operations)
                    operation.Behaviors.Find<DataContractSerializerOperationBehavior>().MaxItemsInObjectGraph = int.MaxValue;

                _channel = channelFactory.CreateChannel();
                ((IClientChannel)_channel).Opened += OnChannelOpened;
                ((IClientChannel)_channel).Closed += OnChannelClosed;
                ((IClientChannel)_channel).Faulted += OnChannelFaulted;
                ((IClientChannel)_channel).Closing += OnChannelClosing;
                ((IClientChannel)_channel).Opening += OnChannelOpening;
            }
        }

        /// <summary>
        /// Previously, a binding was created in the same method as the creation of the "Proxy".  
        /// We seperate the binding in another method call her, in case we want to change the binding in the future, without disturbing the calling parent.
        /// </summary>
        /// <param name="sendTimeout"></param>
        /// <returns></returns>
        private NetTcpBinding CreateBinding(TimeSpan sendTimeout) 
        {
            var binding = new NetTcpBinding(SecurityMode.Transport)
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                SendTimeout = sendTimeout,
                ReliableSession = {InactivityTimeout = TimeSpan.MaxValue, Enabled = true},
                MaxBufferPoolSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                MaxConnections = 10000,
                MaxReceivedMessageSize = int.MaxValue,
                ReaderQuotas =
                {
                    MaxArrayLength = int.MaxValue,
                    MaxBytesPerRead = int.MaxValue,
                    MaxDepth = int.MaxValue,
                    MaxNameTableCharCount = int.MaxValue,
                    MaxStringContentLength = int.MaxValue
                }
            };
            return binding;
        }

        //we need to return closed here when an error occurs because we have legacy code that doesn't handle exceptions properly, but instead uses the return value of TAMResult2 to handle errors (...a procedural methodology).
        public ServiceStatus EnsureChannel()
        {
            try
            {
                if (_channel == null)
                    return ServiceStatus.Closed;
                Task<ServiceStatus> task = new Task<ServiceStatus>(() =>
                {
                    try
                    {
                        return _channel.GetStatus(); //if this returns it will return ServiceStatus.Open
                    }
                    catch (CommunicationObjectAbortedException commEx)
                    {
                        log.Error(commEx);
                        //The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it has been Aborted.
                        return ServiceStatus.Closed;
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                        return ServiceStatus.Closed;
                        //the operation timed out, or the service is not running.  see log for more detail.  FYI...We don't care about the other statuses (e.g. ServiceStatus.Idle), because they are not coded for anywhere in Maestro.
                    }
                }, CancellationToken.None, TaskCreationOptions.None);
                var timeoutTask = task.TimeoutAfter(GetStatusTimeout*1000);
                task.Start(TaskScheduler.Default);
                timeoutTask.Wait();
                return timeoutTask.Result;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Abort();
                return ServiceStatus.Closed;
            }
        }

        /// <summary>
        /// Each call to the Service has it's own timeout property.
        /// The default property is int.MaxValue
        /// Here we can permanently or temoporarily modify the period it takes for the client operation to reach the service,
        /// And return a value.
        /// </summary>
        public TimeSpan OperationTimeout
        {
            get
            {
                return ((IClientChannel)_channel).OperationTimeout;
            }
        }

        /// <summary>
        /// For when we want to close the entire service itself. Essentially making the XXXClient useless except for callbacks not yet in reciept.
        /// </summary>
        public void Close()
        {
            _instanceContext.Close();
        }

        /// <summary>
        /// We can open an XXXClient at will using this property.  Provided it is in the correct state for opening.  The XXXClient can determine this using the XXXClient.State property
        /// </summary>
        public void Open()
        {
            _instanceContext.Open();
        }

        /// <summary>
        /// Calling abort on the InstanceContext of this class disposes of the InstanceContext, and renders this class (TamProxy) unusable.  The Garbage Collector should take care of disposal.
        /// </summary>
        public void Abort()
        {
            _instanceContext.Abort();
             OnContextAborted(this, EventArgs.Empty);
        }

        public CommunicationState State
        {
            get
            {
                return _instanceContext.State;
            }
        }

        private void OnContextOpened(object sender, EventArgs e)
        {
            log.Debug("Proxy.OnContextOpened");
            e.Raise(sender, ref ContextOpened);
        }
        private void OnContextClosed(object sender, EventArgs e)
        {
            log.Debug("Proxy.OnContextClosed");
            e.Raise(sender, ref ContextClosed);
        }
        private void OnContextFaulted(object sender, EventArgs e)
        {
            log.Debug("Proxy.OnContextFaulted");
            e.Raise(sender, ref ContextFaulted);
        }

        private void OnContextAborted(object sender, EventArgs e)
        {
            log.Debug("Proxy.OnContextAborted");
            e.Raise(sender, ref ContextAborted);
        }

        private void OnChannelOpened(object sender, EventArgs e)
        {
            log.Debug("Proxy.OnChannelOpened");
            e.Raise(sender, ref ChannelOpened);
        }
        private void OnChannelClosed(object sender, EventArgs e)
        {
            log.Debug("Proxy.OnChannelClosed");
            e.Raise(sender, ref ChannelClosed);
        }
        private void OnChannelFaulted(object sender, EventArgs e)
        {
            log.Debug("Proxy.OnChannelFaulted");
            e.Raise(sender, ref ChannelFaulted);
        }

        private void OnChannelOpening(object sender, EventArgs e)
        {
            log.Debug("Proxy.OnChannelOpening");
            e.Raise(sender, ref ChannelOpening);
        }

        private void OnChannelClosing(object sender, EventArgs e)
        {
            log.Debug("Proxy.OnChannelClosing");
            e.Raise(sender, ref ChannelClosing);
        }

        public void Dispose()
        {
            try
            {
                if (_channel != null)
                {
                    ((IClientChannel)_channel).Opening -= OnChannelOpening;
                    ((IClientChannel)_channel).Opened -= OnChannelOpened;
                    ((IClientChannel)_channel).Closing -= OnChannelClosing;
                    ((IClientChannel)_channel).Closed -= OnChannelClosed;
                    ((IClientChannel)_channel).Faulted -= OnChannelFaulted;
                    if (((IClientChannel)_channel).State == CommunicationState.Faulted)
                    {
                        ((IClientChannel)_channel).Abort();
                    }
                    else
                    {
                        ((IClientChannel)_channel).Close();
                    }
                    ((IDisposable)_channel).Dispose();
                }
                
                if (_instanceContext != null)
                {
                    _instanceContext.Closed -= OnContextClosed;
                    _instanceContext.Faulted -= OnContextFaulted;
                    _instanceContext.Opened -= OnContextOpened;
                    _instanceContext.Close();
                    if (_instanceContext.State == CommunicationState.Faulted)
                    {
                        _instanceContext.Abort();
                    }
                    else
                    {
                        _instanceContext.Close();
                    }
                }
                _channel = default(T); //default T is null
                
            }
            catch (Exception ex)
            {
                log.Error(ex);
            } 
        }
    }
}

