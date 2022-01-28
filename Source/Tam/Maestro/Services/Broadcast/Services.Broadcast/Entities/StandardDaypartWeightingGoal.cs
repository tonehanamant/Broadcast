namespace Services.Broadcast.Entities
{
    public class StandardDaypartWeightingGoal
    {
        public int StandardDaypartId { get; set; }
        public double WeightingGoalPercent { get; set; }
        public int? DaypartOrganizationId { get; set; }
        public string CustomName { get; set; }
        public string DaypartOrganizationName { get; set; }
        public string DaypartUniquekey { get { return $"{StandardDaypartId}|{DaypartOrganizationId}|{CustomName?.ToLower()}"; } }
    }
}
