using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PlanBuyingRequest
    {
        public int Id { get; set; }
        public List<PlanBuyingRequestDetails> Details{ get; set; }
    }
}
