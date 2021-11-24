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
    }
}
