using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.SystemComponentParameters
{
    public static class SafeBroadcastServiceSystemParameter
    {
        public static bool EnableCampaignsLocking
        {
            get
            {
                try
                {
                    return BroadcastServiceSystemParameter.EnableCampaignsLocking;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
