using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class ApiItemResponseTyped<T> : ApiResponse
    {
        [JsonProperty("result")]
        public T Result { get; set; }
    }
}
