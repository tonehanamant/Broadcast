using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace Tam.Maestro.Web.Common.AppStart
{
    public class UnityWebApiResolver : IDependencyResolver
    {
        private IUnityContainer _container;

        public UnityWebApiResolver(IUnityContainer container)
        {
            _container = container;
        }

        public IDependencyScope BeginScope()
        {
            var child = _container.CreateChildContainer();
            return new UnityWebApiResolver(child);
        }

        public object GetService(Type serviceType)
        {
            try
            {
                if (serviceType.FullName == "System.Web.Http.Metadata.ModelMetadataProvider" ||
                    serviceType.FullName == "System.Web.Http.Validation.IModelValidatorCache")
                {
                    return null;
                }
                return _container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return new List<object>();
            }
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}