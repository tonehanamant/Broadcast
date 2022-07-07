using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Locking
{
    /// <summary>
    /// Represents the locking api response
    /// </summary>
    public class LockingApiResponse
    {
        /// <summary>
        /// Returns True if api gives response otherwise false
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }
        /// <summary>
        /// Returns message as the api response
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
        /// <summary>
        /// Defines the message as severity
        /// </summary>
        [JsonProperty("severity")]
        public string Severity { get; set; }
        /// <summary>
        /// Defines the transactionId of the locking response
        /// </summary>
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }
        /// <summary>
        /// Defines the maskPayload of the locking response
        /// </summary>
        [JsonProperty("maskPayload")]
        public bool MaskPayLoad { get; set; }
    }
}
