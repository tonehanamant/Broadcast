using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class ShareOfVoice
    {
        [JsonProperty("market_code")]
        public int MarketCode { get; set; }

        [JsonProperty("market_goal")]
        public double MarketGoal { get; set; }
    }
}
