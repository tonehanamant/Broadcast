using System;

namespace Services.Broadcast.Helpers
{
    public static class PlanPricingBuyingFileHelper
    {
        public static string GetRequestFileName(string environmentName, 
            int planId, int jobId, string apiVersionNumber,
            DateTime currentDateTime)
        {
            var safeEnvironmentName = string.IsNullOrWhiteSpace(environmentName?.Trim()) ? "UNK" : environmentName?.Trim();
            var safeApiVersionNumber = string.IsNullOrWhiteSpace(apiVersionNumber?.Trim()) ? "UNK" : $"v{apiVersionNumber?.Trim()}";
            var timestampString = currentDateTime.ToString("yyyyMMdd_HHmmss");

            var fileName = $"{safeEnvironmentName}_{safeApiVersionNumber}-request-{planId}_{jobId}-{timestampString}.log";
            return fileName;
        }
    }
}