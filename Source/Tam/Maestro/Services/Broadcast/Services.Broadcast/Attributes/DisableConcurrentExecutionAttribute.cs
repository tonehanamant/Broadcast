using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Server;

namespace Services.Broadcast.Attributes
{
    public class DisableConcurrentExecutionAttribute : JobFilterAttribute, IServerFilter
    {
        private readonly int _timeoutInSeconds;

        public DisableConcurrentExecutionAttribute(int timeoutInSeconds)
        {
            if (timeoutInSeconds < 0) throw new ArgumentException("Timeout argument value should be greater that zero.");

            _timeoutInSeconds = timeoutInSeconds;
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            var resource = GetResource(filterContext.BackgroundJob.Job);

            var timeout = TimeSpan.FromSeconds(_timeoutInSeconds);

            var distributedLock = filterContext.Connection.AcquireDistributedLock(resource, timeout);
            filterContext.Items["DistributedLock"] = distributedLock;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            if (!filterContext.Items.ContainsKey("DistributedLock"))
            {
                throw new InvalidOperationException("Can not release a distributed lock: it was not acquired.");
            }

            var distributedLock = (IDisposable)filterContext.Items["DistributedLock"];
            distributedLock.Dispose();
        }

        private static string GetFingerprint(Job job)
        {
            var parameters = string.Empty;
            if (job?.Args != null)
            {
                parameters = string.Join(".", job.Args.Select(SerializationHelper.Serialize));
            }
            if (job?.Type == null || job.Method == null)
            {
                return string.Empty;
            }
            var payload = $"{job.Type.FullName}.{job.Method.Name}.{parameters}";
            var hash = SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
            var fingerprint = Convert.ToBase64String(hash);
            return fingerprint;
        }
        private static string GetResource(Job job)
        {
            return GetFingerprint(job);
        }
    }
}
