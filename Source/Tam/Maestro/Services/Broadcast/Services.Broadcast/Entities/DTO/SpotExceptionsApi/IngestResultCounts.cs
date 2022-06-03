namespace Services.Broadcast.Entities.DTO.SpotExceptionsApi
{
    public class IngestResultCounts
    {
        public int RecommendedPlansCount { get; set; }
        public int OutOfSpecCount { get; set; }
        public int UnpostedNoPlanCount { get; set; }
        public int UnpostedNoReelRosterCount { get; set; }

        public int TotalCount => RecommendedPlansCount +
                                          OutOfSpecCount +
                                          UnpostedNoPlanCount +
                                          UnpostedNoReelRosterCount;
    }
}
