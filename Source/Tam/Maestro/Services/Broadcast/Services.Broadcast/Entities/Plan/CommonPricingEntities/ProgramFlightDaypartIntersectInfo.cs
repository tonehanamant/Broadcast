namespace Services.Broadcast.Entities.Plan.CommonPricingEntities
{
    public class ProgramFlightDaypartIntersectInfo
    {
        public ProgramInventoryDaypart ProgramInventoryDaypart { get; set; }
        public int SingleIntersectionTime { get; set; }
        public int TotalIntersectingTime { get; set; }
    }
}