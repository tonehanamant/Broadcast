using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Unity;

namespace Tam.Maestro.Web.Common.AppStart
{
    public class UnityWebMvcResolver : IDependencyResolver
    {
        private readonly UnityContainer _container;

        public UnityWebMvcResolver(UnityContainer container)
        {
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            try
            {
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
    }
}