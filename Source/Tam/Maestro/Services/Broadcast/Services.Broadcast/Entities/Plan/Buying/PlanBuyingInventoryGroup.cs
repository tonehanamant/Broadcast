namespace Services.Broadcast.Entities.Plan.Buying
{
    public class PlanBuyingInventoryGroup
    {
        public int StationId { get; set; }

        public int DaypartId { get; set; }

        public string PrimaryProgramName { get; set; }

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType())
                return false;
            return Equals((PlanBuyingInventoryGroup)other);
        }

        public override int GetHashCode()
        {
            return StationId.GetHashCode() + DaypartId.GetHashCode() + PrimaryProgramName.GetHashCode();
        }

        public bool Equals(PlanBuyingInventoryGroup other)
        {
            return other.StationId == StationId &&
                   other.DaypartId == DaypartId &&
                   other.PrimaryProgramName == PrimaryProgramName;
        }
    }
}
