namespace Services.Broadcast.Entities.Enums
{
    /// <summary>
    /// Identifies a mode the Spot Allocation Model.
    /// </summary>
    public enum SpotAllocationModelMode
    {
        /// <summary>
        /// The model looks to hit the given CPM Goal.
        /// </summary>
        Quality = 1,
        /// <summary>
        /// The model looks for the most efficient CPM.
        /// </summary>
        Efficiency = 2,
        /// <summary>
        /// The model looks for the cheapest CPM.
        /// </summary>
        Floor = 3
    }
}