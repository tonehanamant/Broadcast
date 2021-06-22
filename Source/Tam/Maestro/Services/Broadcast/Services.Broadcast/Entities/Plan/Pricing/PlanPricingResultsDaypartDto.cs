using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Plan.Pricing
{
   public class PlanPricingResultsDaypartDto
    {
        public int Id { get; set; }
        public int PlanVersionPricingResultId { get; set; }
        public int StandardDaypartId { get; set; }
        public double CalculatedVpvh { get; set; }

      
    }
}
