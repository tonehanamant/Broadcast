using Newtonsoft.Json;
using System;

namespace Services.Broadcast.Entities
{
    public class RegisterResponseDto
    {
        [JsonProperty("result")]
        public Guid AttachmentId { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("severity")]
        public string Severity { get; set; }
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }
    }
}
