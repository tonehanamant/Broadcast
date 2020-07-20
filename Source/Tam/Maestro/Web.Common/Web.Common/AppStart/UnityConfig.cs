using Common.Services;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using Unity;
using Unity.Injection;

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

            // null registrations to prevent resolution failed exceptions from being thrown

            container.RegisterFactory<ITraceManager>(c => null);
            container.RegisterFactory<ITraceWriter>(c => null);
            container.RegisterFactory<IHttpControllerSelector>(c => null);
            container.RegisterFactory<IAssembliesResolver>(c => null);
            container.RegisterFactory<IHttpControllerTypeResolver>(c => null);
            container.RegisterFactory<IHttpActionSelector>(c => null);
            container.RegisterFactory<IActionValueBinder>(c => null);
            container.RegisterFactory<IBodyModelValidator>(c => null);
            container.RegisterFactory<IHostBufferPolicySelector>(c => null);
            container.RegisterFactory<IControllerFactory>(c => null);
            container.RegisterFactory<IControllerActivator>(c => null);
            container.RegisterFactory<ITempDataProviderFactory>(c => null);
            container.RegisterFactory<ITempDataProvider>(c => null);
            container.RegisterFactory<IAsyncActionInvokerFactory>(c => null);
            container.RegisterFactory<IActionInvokerFactory>(c => null);
            container.RegisterFactory<IAsyncActionInvoker>(c => null);
            container.RegisterFactory<IActionInvoker>(c => null);
            container.RegisterFactory<IViewPageActivator>(c => null);
            container.RegisterFactory<IHttpControllerActivator>(c => null);
            container.RegisterFactory<IHttpActionInvoker>(c => null);
            container.RegisterFactory<IContentNegotiator>(c => null);
            container.RegisterFactory<IExceptionHandler>(c => null);

            // these types will need some other type of handling
            //container.RegisterType<ModelMetadataProvider>(new InjectionFactory((c) => null));
            //container.RegisterType<IModelValidatorCache>(new InjectionFactory((c) => null));
        }
    }
}