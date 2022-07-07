using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Locking
{
    /// <summary>
    /// Represetns the release api response
    /// </summary>
    public class ReleaseApiResponse
    {
        /// <summary>
        /// Returns True if api is successful otherwise false
        /// </summary>
        [JsonProperty("result")]
        public bool Result { get; set; }
        /// <summary>
        /// Returns true if returns api reposne otherwise false
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }
        /// <summary>
        /// Defines the severity
        /// </summary>
        [JsonProperty("severity")]
        public string Severity { get; set; }
        /// <summary>
        /// Defines the message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

    }
}
