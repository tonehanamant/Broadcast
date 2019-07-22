using Common.Services;
using Common.Services.WebComponents;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.Unity;
using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Tam.Maestro.Services.Cable;
using Tam.Maestro.Web.Common.AppStart;

namespace BroadcastComposerWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private UnityContainer _container;
        private IWebLogger _logger;
        private ObservableEventListener _logListener;

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            _container = new UnityContainer();
            UnityConfig.RegisterTypes(_container);
            _container.RegisterType<IWebLogger, BroadcastWebLogger>();

            _logger = _container.Resolve<IWebLogger>();
            _logger.LogEventInformation("Initializing Broadcast Web Application.", "BroadcastController");

            GlobalConfiguration.Configuration.DependencyResolver = new UnityWebApiResolver(_container);
            DependencyResolver.SetResolver(new UnityWebMvcResolver(_container));

            AreaRegistration.RegisterAllAreas();

            //Enable CORS
            string[] allowedOrigins = { "http://localhost", "https://localhost", "http://localhost:9015", "https://localhost:9015", "http://localhost:9016", "https://localhost:9016", "http://localhost:9017", "https://localhost:9017", "http://localhost:9018", "https://localhost:9018", "http://localhost:9019", "https://localhost:9019", "http://localhost:9020", "https://localhost:9020" };
            string[] allowedHeaders = { "Origin", "Content-Type", "Accept" };
            string[] allowedMethods = {"GET", "POST", "PUT", "DELETE", "OPTIONS"};
            var cors = new EnableCorsAttribute(
                        origins: string.Join(", ", allowedOrigins),
                        headers: string.Join(", ", allowedHeaders),
                        methods: string.Join(", ", allowedMethods));
            cors.SupportsCredentials = true;
            GlobalConfiguration.Configuration.EnableCors(cors);

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var config = _container.Resolve<IConfiguration>();

            string _logRequests = WebConfigurationManager.AppSettings["logRequests"];
            bool logRequests;
            if (!Boolean.TryParse(_logRequests, out logRequests))
                logRequests = false; //By default, do not log requests if unable to parse the config
            if (logRequests)
                GlobalConfiguration.Configuration.MessageHandlers.Add(new MessageHandler.MessageLoggingHandler(_logger));

            _logListener = new ObservableEventListener();
            _logListener.LogToConsole();
            var disableSlabInProcess = (ConfigurationManager.AppSettings["DisableSlabInProcess"] == "true");
            if (!disableSlabInProcess)
            {
                _logListener.LogToRollingFlatFile(config.LogFilePath, 102400, "yyyyMMdd",
                    Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks.RollFileExistsBehavior.Increment,
                    Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks.RollInterval.Day);
            }

            _logger.LogEventInformation("Broadcast Web Application Initialized.", "BroadcastController");
        }
    }
}
