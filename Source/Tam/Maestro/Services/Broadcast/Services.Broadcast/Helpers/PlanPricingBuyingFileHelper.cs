using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Helpers
{
    public static class PlanPricingBuyingFileHelper
    {
        public static string GetRequestFileName(string environmentName, 
            int planId, int jobId, string apiVersionNumber,
            DateTime currentDateTime, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var safeEnvironmentName = string.IsNullOrWhiteSpace(environmentName?.Trim()) ? "UNK" : environmentName?.Trim();
            var safeApiVersionNumber = string.IsNullOrWhiteSpace(apiVersionNumber?.Trim()) ? "UNK" : $"v{apiVersionNumber?.Trim()}";
            var timestampString = currentDateTime.ToString("yyyyMMdd_HHmmss");
            var allocationMode = spotAllocationModelMode.ToString().Substring(0, 1);

            var fileName = $"{safeEnvironmentName}_{safeApiVersionNumber}-request-{planId}_{jobId}_{allocationMode}-{timestampString}.log";
            return fileName;
        }
    }
}