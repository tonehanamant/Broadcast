using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class ApiResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("severity")]
        public string Severity { get; set; }
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }
    }
    public class InventoryExportApiResponse
    {
        public bool Success { get; set; }
        public object Message { get; set; }
        public string Data { get; set; }
    }
}
