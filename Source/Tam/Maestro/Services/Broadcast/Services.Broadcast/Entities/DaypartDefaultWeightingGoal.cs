namespace Services.Broadcast.Entities
{
    public class DaypartDefaultWeightingGoal
    {
        public DaypartDefaultWeightingGoal()
        {
        }

        public DaypartDefaultWeightingGoal(int daypartDefaultId, double weightingGoalPercent)
        {
            DaypartDefaultId = daypartDefaultId;
            WeightingGoalPercent = weightingGoalPercent;
        }

        public int DaypartDefaultId { get; set; }
        public double WeightingGoalPercent { get; set; }
    }
}
