using Remotion.Linq.Clauses.ResultOperators;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanMappingDto
    {
        public int PlanId { get; set; }
        public string Isci { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var candidate = (IsciPlanMappingDto)obj;
            var result = candidate.PlanId == PlanId && 
                         candidate.Isci == Isci;
            return result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
