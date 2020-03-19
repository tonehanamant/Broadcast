using Newtonsoft.Json;
using System;

namespace BroadcastJobScheduler
{
    /// <summary>
    /// Scheduler extensions.
    /// </summary>
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Converts the serializable object to Json string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToJson(this object data)
        {
            try
            {
                return JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Serialization failed for the object with exception: {ex.Message}");
            }
        }
    }
}
