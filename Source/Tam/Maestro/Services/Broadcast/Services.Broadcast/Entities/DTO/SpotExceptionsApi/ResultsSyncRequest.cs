using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace Services.Broadcast.Entities.DTO.SpotExceptionsApi
{
    public class ResultsSyncRequest
    {
        [JsonProperty(PropertyName = "requested_by")]
        public string RequestedBy { get; set; }
    }
}
