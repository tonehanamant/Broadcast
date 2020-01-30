using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PlanBuyingInventoryPlanner
    {
        public int Id { get; set; }
        public List<PlanBuyingInventoryPlannerDetail> Details { get; set; }
    }
}
