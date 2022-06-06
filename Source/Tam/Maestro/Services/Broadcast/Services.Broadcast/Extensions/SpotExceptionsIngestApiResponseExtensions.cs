using Services.Broadcast.Entities.DTO.SpotExceptionsApi;

namespace Services.Broadcast.Extensions
{
    internal static class SpotExceptionsIngestApiResponseExtensions
    {
        public static void MergeIngestApiResponses(this IngestApiResponse result, IngestApiResponse from)
        {
            result.JobId = from.JobId;
            result.SkipClearStaged = from.SkipClearStaged;
            result.SkipIngestAndStaged = from.SkipIngestAndStaged;

            result.ReceivedCounts.RecommendedPlansCount += from.ReceivedCounts.RecommendedPlansCount;
            result.ReceivedCounts.OutOfSpecCount += from.ReceivedCounts.OutOfSpecCount;
            result.ReceivedCounts.UnpostedNoPlanCount += from.ReceivedCounts.UnpostedNoPlanCount;
            result.ReceivedCounts.UnpostedNoReelRosterCount += from.ReceivedCounts.UnpostedNoReelRosterCount;

            result.StagedCounts.RecommendedPlansCount += from.StagedCounts.RecommendedPlansCount;
            result.StagedCounts.OutOfSpecCount += from.StagedCounts.OutOfSpecCount;
            result.StagedCounts.UnpostedNoPlanCount += from.StagedCounts.UnpostedNoPlanCount;
            result.StagedCounts.UnpostedNoReelRosterCount += from.StagedCounts.UnpostedNoReelRosterCount;

            result.ProcessedCounts.RecommendedPlansCount += from.ProcessedCounts.RecommendedPlansCount;
            result.ProcessedCounts.OutOfSpecCount += from.ProcessedCounts.OutOfSpecCount;
            result.ProcessedCounts.UnpostedNoPlanCount += from.ProcessedCounts.UnpostedNoPlanCount;
            result.ProcessedCounts.UnpostedNoReelRosterCount += from.ProcessedCounts.UnpostedNoReelRosterCount;
        }
    }
}
