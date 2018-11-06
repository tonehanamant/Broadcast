namespace Services.Broadcast.Entities
{
    public class AllocationGoal
    {
        public AllocationGoal() { }

        public AllocationGoal(decimal? budgetGoal, double? impressionGoal)
        {
            BudgetGoal = budgetGoal;
            ImpressionGoal = impressionGoal;
        }

        public decimal? BudgetGoal { get; set; }
        public double? ImpressionGoal { get; set; }
    }
}
