using System;

namespace Services.Broadcast.Helpers
{
    public static class PlanPricingBuyingFileHelper
    {
        public static string GetRequestFileName(string environmentName, 
            int planId, int jobId, DateTime currentDateTime)
        {
            var timestampString = currentDateTime.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{environmentName}-request-{planId}_{jobId}-{timestampString}.log";
            return fileName;
        }
    }
}