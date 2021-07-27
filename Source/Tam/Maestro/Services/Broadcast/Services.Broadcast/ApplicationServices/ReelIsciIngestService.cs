using Common.Services.ApplicationServices;
using Hangfire;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.ReelRosterIscis;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IReelIsciIngestService : IApplicationService
    {
        /// <summary>
        /// Allows to test the reel isci client.
        /// </summary>
        List<ReelRosterIsciDto> TestReelISciApiClient(DateTime startDate, int numberOfDays);
        /// <summary>
        /// Perform reel isci ingest.
        /// </summary>
        [Queue("reelisciingest")]
        void PerformReelIsciIngest(string userName);
    }

    public class ReelIsciIngestService : BroadcastBaseClass, IReelIsciIngestService
    {
        private readonly IReelIsciApiClient _ReelIsciApiClient;

        public ReelIsciIngestService(IReelIsciApiClient reelIsciApiClient)
        {
            _ReelIsciApiClient = reelIsciApiClient;
        }

        /// <inheritdoc />
        public List<ReelRosterIsciDto> TestReelISciApiClient(DateTime startDate, int numberOfDays)
        {
            _LogInfo($"Calling RealIsciClient. startDate='{startDate.ToString(ReelIsciApiClient.ReelIsciApiDateFormat)}';numberOfDays='{numberOfDays}'");

            var result = _ReelIsciApiClient.GetReelRosterIscis(startDate, numberOfDays);

            _LogInfo($"Received a response containing '{result.Count}' records.");
            return result;
        }
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void PerformReelIsciIngest(string userName)
        {
            _LogInfo($"executing...... .", userName);
        }
    }
}
