using Common.Services;
using Common.Services.WebComponents;
using Microsoft.Practices.Unity;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace Tam.Maestro.Web.Common.AppStart
{
    public class UnityConfig
    {
        public static void RegisterTypes(UnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below...
            // container.LoadConfiguration();

            // TODO: Register your types here
            container.RegisterType<IConfiguration, FileBasedConfiguration>();

            // null registrations to prevnent resolution failed exceptions from being thrown
            container.RegisterType<ITraceManager>(new InjectionFactory((c) => null));
            container.RegisterType<ITraceWriter>(new InjectionFactory((c) => null));
            container.RegisterType<IHttpControllerSelector>(new InjectionFactory((c) => null));
            container.RegisterType<IAssembliesResolver>(new InjectionFactory((c) => null));
            container.RegisterType<IHttpControllerTypeResolver>(new InjectionFactory((c) => null));
            container.RegisterType<IHttpActionSelector>(new InjectionFactory((c) => null));
            container.RegisterType<IActionValueBinder>(new InjectionFactory((c) => null));
            container.RegisterType<IBodyModelValidator>(new InjectionFactory((c) => null));
            container.RegisterType<IHostBufferPolicySelector>(new InjectionFactory((c) => null));
            container.RegisterType<IControllerFactory>(new InjectionFactory((c) => null));
            container.RegisterType<IControllerActivator>(new InjectionFactory((c) => null));
            container.RegisterType<ITempDataProviderFactory>(new InjectionFactory((c) => null));
            container.RegisterType<ITempDataProvider>(new InjectionFactory((c) => null));
            container.RegisterType<IAsyncActionInvokerFactory>(new InjectionFactory((c) => null));
            container.RegisterType<IActionInvokerFactory>(new InjectionFactory((c) => null));
            container.RegisterType<IAsyncActionInvoker>(new InjectionFactory((c) => null));
            container.RegisterType<IActionInvoker>(new InjectionFactory((c) => null));
            container.RegisterType<IViewPageActivator>(new InjectionFactory((c) => null));
            container.RegisterType<IHttpControllerActivator>(new InjectionFactory((c) => null));
            container.RegisterType<IHttpActionInvoker>(new InjectionFactory((c) => null));
            container.RegisterType<IContentNegotiator>(new InjectionFactory((c) => null));
            container.RegisterType<IExceptionHandler>(new InjectionFactory((c) => null));
            // these types will need some other type of handling
            //container.RegisterType<ModelMetadataProvider>(new InjectionFactory((c) => null));
            //container.RegisterType<IModelValidatorCache>(new InjectionFactory((c) => null));
        }
    }
}