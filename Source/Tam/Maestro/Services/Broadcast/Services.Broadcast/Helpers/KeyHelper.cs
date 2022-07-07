using Services.Broadcast.Entities;

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
        /// <summary>
        /// Get the locking request
        /// </summary>
        /// <param name="key">key passes as parameter</param>
        /// <returns>Locking api request</returns>
        public static LockingApiRequest GetLokcingRequest(string key)
        {
            string[] lockKeyArray = key.Split(':');
            return new LockingApiRequest
            {
                ObjectType = lockKeyArray[0],
                ObjectId = lockKeyArray[1],
                ExpirationTimeSpan = System.TimeSpan.FromMinutes(15),
                SharedApplications = null,
                IsShared = false
            };
        }
    }
}
