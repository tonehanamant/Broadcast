using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.IntegrationTests.Stubs;
using System;
using System.Collections.Generic;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class ProgramGuideServiceIntegrationTests
    {
        private IProgramGuideService _ProgramGuideService;

        [SetUp]
        public void SetUp()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IProgramGuideApiClient, ProgramGuideApiClientStub>();
            _ProgramGuideService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProgramGuideService>();
        }

        [Test]
        public void PerformGetProgramsForGuideTest_CanCallE2E()
        {
            var result = _ProgramGuideService.PerformGetProgramsForGuideTest();
            Assert.IsNotNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramsForGuide_CanCallE2E()
        {
            const int addDaysStartDate = 30;
            const int addDaysEndDate = addDaysStartDate + 7;
            const string dateTimeFormat = "MM/dd/yyyy";

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
                    EndTime = 14399
                }
            };

            var result = _ProgramGuideService.GetProgramsForGuide(new List<GuideRequestElementDto> { requestElement });

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(requestStartDate.ToString(dateTimeFormat), result[0].StartDate.ToString(dateTimeFormat));
            Assert.AreEqual(requestEndDate.ToString(dateTimeFormat), result[0].EndDate.ToString(dateTimeFormat));
            Assert.IsNotNull(result[0].Programs);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(GuideResponseElementDto), "Programs");
            jsonResolver.Ignore(typeof(GuideResponseElementDto), "StartDate");
            jsonResolver.Ignore(typeof(GuideResponseElementDto), "EndDate");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramsForGuide_TwoWeeksBackThreeWeeksForward()
        {
            const int addDaysStartDate = -14;  // 2 weeks back
            const int addDaysEndDate = 21;  // 3 weeks forward
            const string dateTimeFormat = "MM/dd/yyyy";

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
                    EndTime = 14399
                }
            };

            var result = _ProgramGuideService.GetProgramsForGuide(new List<GuideRequestElementDto> { requestElement });

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(requestStartDate.ToString(dateTimeFormat), result[0].StartDate.ToString(dateTimeFormat));
            Assert.AreEqual(requestEndDate.ToString(dateTimeFormat), result[0].EndDate.ToString(dateTimeFormat));
            Assert.IsNotNull(result[0].Programs);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(GuideResponseElementDto), "Programs");
            jsonResolver.Ignore(typeof(GuideResponseElementDto), "StartDate");
            jsonResolver.Ignore(typeof(GuideResponseElementDto), "EndDate");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        public void GetProgramsForGuide_FailValidation_NetworkAffiliate()
        {
            const int addDaysStartDate = 7; 
            const int addDaysEndDate = addDaysStartDate + 7;

            var requestStartDate = DateTime.Now.AddDays(addDaysStartDate);
            var requestEndDate = DateTime.Now.AddDays(addDaysEndDate);

            var requestElement = new GuideRequestElementDto
            {
                Id = "RequestScenarioOne",
                StartDate = requestStartDate,
                EndDate = requestEndDate,
                StationCallLetters = "WGHP-DT",
                NetworkAffiliate = null,
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
                    EndTime = 14399
                }
            };
            var expectedErrorMessage = $"Bad Request.  Request '{requestElement.Id}' requires a NetworkAffiliate.";

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _ProgramGuideService.GetProgramsForGuide(new List<GuideRequestElementDto> {requestElement}));
            Assert.AreEqual(ex.Message, expectedErrorMessage);
        }

        [Test]
        public void GetProgramsForGuide_FailValidation_StationCallLetters()
        {
            const int addDaysStartDate = 7;
            const int addDaysEndDate = addDaysStartDate + 7;

            var requestStartDate = DateTime.Now.AddDays(addDaysStartDate);
            var requestEndDate = DateTime.Now.AddDays(addDaysEndDate);

            var requestElement = new GuideRequestElementDto
            {
                Id = "RequestScenarioOne",
                StartDate = requestStartDate,
                EndDate = requestEndDate,
                StationCallLetters = null,
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
                    EndTime = 14399
                }
            };
            var expectedErrorMessage = $"Bad Request.  Request '{requestElement.Id}' requires StationCallLetters.";

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _ProgramGuideService.GetProgramsForGuide(new List<GuideRequestElementDto> { requestElement }));
            Assert.AreEqual(ex.Message, expectedErrorMessage);
        }

        [Test]
        public void GetProgramsForGuide_ManyElements()
        {
            int maxChunkSize = ProgramGuideApiClient.RequestElementMaxCount.Value;
            const int addDaysStartDate = 30;
            const int addDaysEndDate = addDaysStartDate + 7;

            var requestStartDate = DateTime.Now.AddDays(addDaysStartDate);
            var requestEndDate = DateTime.Now.AddDays(addDaysEndDate);
            var requests = new List<GuideRequestElementDto>();
            for (int i = 0; i < maxChunkSize; i++)
            {
                requests.Add(new GuideRequestElementDto
                {
                    Id = $"RequestScenario{i}",
                    StartDate = requestStartDate,
                    EndDate = requestEndDate,
                    StationCallLetters = "WGHP-DT",
                    NetworkAffiliate = "FOX",
                    Daypart = new GuideRequestDaypartDto
                    {
                        Id = $"TestElement{i}_Daypart",
                        Name = "TestDaypartText1",
                        Monday = true,
                        Tuesday = true,
                        Wednesday = true,
                        Thursday = true,
                        Friday = true,
                        Saturday = false,
                        Sunday = false,
                        StartTime = 7200,
                        EndTime = 14399
                    }
                });
            }

            var result = _ProgramGuideService.GetProgramsForGuide(requests);
            Assert.IsNotNull(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProgramsForGuide_FailValidation_RequestElementCount()
        {
            int maxChunkSize = ProgramGuideApiClient.RequestElementMaxCount.Value + 1;
            const int addDaysStartDate = 30;
            const int addDaysEndDate = addDaysStartDate + 7;

            var requestStartDate = DateTime.Now.AddDays(addDaysStartDate);
            var requestEndDate = DateTime.Now.AddDays(addDaysEndDate);
            var requests = new List<GuideRequestElementDto>();
            for (int i = 0; i < maxChunkSize; i++)
            {
                requests.Add(new GuideRequestElementDto
                {
                    Id = $"RequestScenario{i}",
                    StartDate = requestStartDate,
                    EndDate = requestEndDate,
                    StationCallLetters = "WGHP-DT",
                    NetworkAffiliate = "FOX",
                    Daypart = new GuideRequestDaypartDto
                    {
                        Id = $"TestElement{i}_Daypart",
                        Name = "TestDaypartText1",
                        Monday = true,
                        Tuesday = true,
                        Wednesday = true,
                        Thursday = true,
                        Friday = true,
                        Saturday = false,
                        Sunday = false,
                        StartTime = 7200,
                        EndTime = 14399
                    }
                });
            }

            var expectedErrorMessage = $"The request element count of {requests.Count} exceeds the max acceptable count of {ProgramGuideApiClient.RequestElementMaxCount}.";

            var ex = Assert.Throws<InvalidOperationException>(() => _ProgramGuideService.GetProgramsForGuide(requests));
            Assert.AreEqual(ex.Message, expectedErrorMessage);
        }
    }
}