using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Helpers
{
    public static class PlanPricingBuyingFileHelper
    {
        /// <summary>
        /// Gets the name of the request file.
        /// </summary>
        /// <param name="environmentName">Name of the environment.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="apiVersionNumber">The API version number.</param>
        /// <param name="currentDateTime">The current date time.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        public static string GetRequestFileName(string environmentName, 
            int planId, int jobId, string apiVersionNumber,
            DateTime currentDateTime, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            var safeEnvironmentName = string.IsNullOrWhiteSpace(environmentName?.Trim()) ? "UNK" : environmentName?.Trim();
            var safeApiVersionNumber = string.IsNullOrWhiteSpace(apiVersionNumber?.Trim()) ? "UNK" : $"v{apiVersionNumber?.Trim()}";
            var timestampString = currentDateTime.ToString("yyyyMMdd_HHmmss");
            var allocationMode = spotAllocationModelMode.ToString().Substring(0, 1);

            var fileName = $"{safeEnvironmentName}_{safeApiVersionNumber}-request-{planId}_{jobId}_{allocationMode}-{timestampString}.json";
            return fileName;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="environmentName">Name of the environment.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="currentDateTime">The current date time.</param>
        /// <returns></returns>
        public static string GetFileName( string environmentName, 
            int planId, int jobId, DateTime currentDateTime)
        {
            var safeEnvironmentName = string.IsNullOrWhiteSpace(environmentName?.Trim()) ? "UNK" : environmentName?.Trim();
            var timestampString = currentDateTime.ToString("yyyyMMdd_HHmmss");

            var fileName = $"{safeEnvironmentName}_BuyingInventory_{planId}_{jobId}_{timestampString}.json";
            return fileName;
        }
    }
}