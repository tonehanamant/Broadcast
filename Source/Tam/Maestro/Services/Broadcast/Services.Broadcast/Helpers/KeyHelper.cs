namespace Services.Broadcast.Helpers
{
    public static class KeyHelper
    {
        public static string GetCampaignLockingKey(int campaignId)
        {
            return $"broadcast_campaign : {campaignId}";
        }

        public static string GetProposalLockingKey(int proposalId)
        {
            return $"broadcast_proposal : {proposalId}";
        }

        public static string GetStationLockingKey(int stationId)
        {
            return $"broadcast_station : {stationId}";
        }

        public static string GetPlanLockingKey(int planId)
        {
            return $"broadcast_plan : {planId}";
        }
    }
}
