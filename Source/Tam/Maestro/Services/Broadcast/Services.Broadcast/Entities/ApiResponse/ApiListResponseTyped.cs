using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class ApiListResponseTyped<T> : ApiResponse
    {
        [JsonProperty("resultList")]
        public List<T> ResultList { get; set; }
    }
}
