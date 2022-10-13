namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    public class SpotExceptionsOutOfSpecDecisionPostsRequestDto
    {
        public int Id { get; set; }
        public bool AcceptAsInSpec { get; set; }
        public string DecisionNotes { get; set; }
    }
}
