using BroadcastLogging;
using log4net;
using Services.Broadcast.ApplicationServices;
using System;
using System.Runtime.CompilerServices;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Tam.Maestro.Web.Common.AppStart;
using Unity;

namespace BroadcastComposerWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private UnityContainer _container;
        private ILog _Log;

        protected void Application_Start()
        {
            _container = new UnityContainer();
            UnityConfig.RegisterTypes(_container);

            /*** Setup logging ***/
            SetupLogging();
            LogInfo("Initializing Broadcast Web Application.");

            GlobalConfiguration.Configuration.DependencyResolver = new UnityWebApiResolver(_container);
            DependencyResolver.SetResolver(new UnityWebMvcResolver(_container));

            AreaRegistration.RegisterAllAreas();

            // Initialize BroadcastApplicationServiceFactory UnityContainer instance so it's available
            // on method OnActionExecuting for the ViewControllerBase.
            var instance = BroadcastApplicationServiceFactory.Instance;

            //Enable CORS
            string[] allowedOrigins = { "http://localhost", "https://localhost", "http://localhost:9015", "https://localhost:9015", "http://localhost:9016", "https://localhost:9016", "http://localhost:9017", "https://localhost:9017", "http://localhost:9018", "https://localhost:9018", "http://localhost:9019", "https://localhost:9019", "http://localhost:9020", "https://localhost:9020", "http://localhost:8080", "https://localhost:8080" };
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

            SetupWebRequestResponseLogging();

            LogInfo("Broadcast Web Application Initialized.");
            LogInfo($"DisableSecurity: {WebConfigurationManager.AppSettings["DisableSecurity"]}");
        }

        private void SetupLogging()
        {
            log4net.Config.XmlConfigurator.Configure();
            _container.RegisterInstance<IBroadcastLoggingConfiguration>(new BroadcastComposerWebLogConfig());
            BroadcastLogMessageHelper.Configuration = _container.Resolve<IBroadcastLoggingConfiguration>();
            _Log = LogManager.GetLogger(typeof(MvcApplication));
        }

        private void SetupWebRequestResponseLogging()
        {
            string _logRequests = WebConfigurationManager.AppSettings["logRequests"];
            bool logRequests;
            if (!Boolean.TryParse(_logRequests, out logRequests))
            {
                logRequests = false; //By default, do not log requests if unable to parse the config
            }

            if (logRequests)
            {
                GlobalConfiguration.Configuration.MessageHandlers.Add(new BroadcastWebLogMessageHandler(_Log));
            }
        }

        private void LogInfo(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Info(logMessage.ToJson());
        }
    }
}
