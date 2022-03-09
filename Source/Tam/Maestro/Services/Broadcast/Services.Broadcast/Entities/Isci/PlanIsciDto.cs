using System;

namespace Services.Broadcast.Entities.Isci
{
    /// <summary>
    /// A dto for repo interaction.
    /// </summary>
    public class PlanIsciDto
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public string Isci { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
        public DateTime ActiveStartDate { get; set; }
        public DateTime ActiveEndDate { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var candidate = (PlanIsciDto)obj;
            var result = candidate.PlanId == PlanId &&
                         candidate.Isci == Isci && 
                         candidate.FlightStartDate.Date.Equals(FlightStartDate.Date) && 
                         candidate.FlightEndDate.Date.Equals(FlightEndDate.Date);
            return result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
