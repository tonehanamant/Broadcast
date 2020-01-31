using Common.Services.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.ProgramGuide;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProgramGuideService : IApplicationService
    {
        List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements);

        List<GuideResponseElementDto> PerformGetProgramsForGuideTest();
    }

    public class ProgramGuideService : IProgramGuideService
    {
        private readonly IProgramGuideApiClient _ProgramGuideApiClient;

        public ProgramGuideService(
            IProgramGuideApiClient programGuideApiClient)
        {
            _ProgramGuideApiClient = programGuideApiClient;
        }

        public List<GuideResponseElementDto> GetProgramsForGuide(List<GuideRequestElementDto> requestElements)
        {
            var result = _ProgramGuideApiClient.GetProgramsForGuide(requestElements);
            return result;
        }

        /// <summary>
        /// Performs an end to end test with a canned request.
        /// Helps to determine if the issue is on our side.
        /// </summary>
        /// <returns></returns>
        public List<GuideResponseElementDto> PerformGetProgramsForGuideTest()
        {
            const int addDaysStartDate = 30;
            const int addDaysEndDate = addDaysStartDate + 7;

            var requestStartDate = DateTime.Now.AddDays(addDaysStartDate);
            var requestEndDate = DateTime.Now.AddDays(addDaysEndDate);

            var requestElement = new GuideRequestElementDto
            {
                Id = "RequestScenarioOne",
                StartDate = requestStartDate,
                EndDate = requestEndDate,
                StationCallLetters = "WGHP-DT",
                NetworkAffiliate = "FOX",
                Daypart = new GuideRequestDaypartDto
                {
                    Id = "TestElement1_Daypart1",
                    Name = "TestDaypartText1",
                    Monday = true,
                    Tuesday = true,
                    Wednesday = true,
                    Thursday = true,
                    Friday = true,
                    Saturday = false,
                    Sunday = false,
                    StartTime = 7200,
                    EndTime = 14400
                }
            };

            var result = GetProgramsForGuide(new List<GuideRequestElementDto> { requestElement });
            return result;
        }
    }
}
