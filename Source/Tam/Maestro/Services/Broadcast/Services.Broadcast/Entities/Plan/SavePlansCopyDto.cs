namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    ///  Dto for Plans Copy data.
    /// </summary>
    public class SavePlansCopyDto
    {
        public int SourcePlanId { get; set; }
        public string Name { get; set; }
        public string ProductMasterId { get; set; }
    }
}
