using Hangfire;
using System;
using Unity;

namespace BroadcastJobScheduler
{
    /// <summary>
    /// Hangfire Job activator
    /// </summary>
    public class HangfireJobActivator : JobActivator
    {
        private readonly IUnityContainer _container;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Unity container</param>
        public HangfireJobActivator(IUnityContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Resolves all required types and creates job of requested type.
        /// </summary>
        /// <param name="type">Job type</param>
        /// <returns>instance of the requested type</returns>
        public override object ActivateJob(Type type)
        {
            return _container.Resolve(type);
        }
    }
}