using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.Vpvh;
using System;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class SpotExceptionsServiceIntegrationTests
    {
        private readonly ISpotExceptionService _SpotExceptionService;

        public SpotExceptionsServiceIntegrationTests()
        {
            _SpotExceptionService = IntegrationTestApplicationServiceFactory.GetApplicationService<ISpotExceptionService>();
        }

        // This is temporary test : TODO: Shaun Cleanup
        //[Test]
        public void AddSpotExceptionData()
        {
            _SpotExceptionService.ClearSpotExceptionMockData();
            _SpotExceptionService.AddSpotExceptionData(isIntegrationTestDatabase: true);

            var weekStartDate = new DateTime(2021, 10, 10);
            var weekEndDate = new DateTime(2021, 10, 17);

            var outofSpecsPostsData = _SpotExceptionService.GetSpotExceptionsOutOfSpecsPosts(new SpotExceptionsOutOfSpecPostsRequestDto
            {
                WeekStartDate = weekStartDate,
                WeekEndDate = weekEndDate
            });

            var outofSpecsPlansData = _SpotExceptionService.GetSpotExceptionsOutofSpecsPlans(new SpotExceptionsOutofSpecsPlansRequestDto
            {
                WeekStartDate = weekStartDate,
                WeekEndDate = weekEndDate
            });

            var outofSpecsPlansSpotsData = _SpotExceptionService.GetSpotExceptionsOutofSpecSpots(new SpotExceptionsOutofSpecSpotsRequestDto
            {
                PlanId = 524,
                WeekStartDate = weekStartDate,
                WeekEndDate = weekEndDate
            });
        }
    }
}
