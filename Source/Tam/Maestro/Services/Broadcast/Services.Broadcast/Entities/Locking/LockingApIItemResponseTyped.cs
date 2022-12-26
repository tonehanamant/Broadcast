using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Locking
{
    /// <summary>
    /// Represents the locking api response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LockingApIItemResponseTyped<T> : LockingApiResponse
    {
        /// <summary>
        /// Defines the api result
        /// </summary>
        [JsonProperty("result")]
        public T Result { get; set; }
    }
}
