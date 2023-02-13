using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.SpotExceptions
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionsOutOfSpecServiceUnitTestsV2
    {
        private SpotExceptionsOutOfSpecServiceV2 _SpotExceptionsOutOfSpecService;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISpotExceptionsOutOfSpecRepository> _SpotExceptionsOutOfSpecRepositoryMock;
        private Mock<ISpotExceptionsOutOfSpecRepositoryV2> _SpotExceptionsOutOfSpecRepositoryV2Mock;
        private Mock<ISpotExceptionsOutOfSpecServiceV2> _SpotExceptionsOutOfSpecServiceV2Mock;
        private Mock<IPlanRepository> _PlanRepositoryMock;

        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IAabEngine> _AabEngine;
        private Mock<IGenreCache> _GenreCacheMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private Mock<IFileService> _FileServicesMock;
        private Mock<ISharedFolderService> _SharedFolderServiceMock;
       
        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotExceptionsOutOfSpecRepositoryMock = new Mock<ISpotExceptionsOutOfSpecRepository>();
            _SpotExceptionsOutOfSpecRepositoryV2Mock = new Mock<ISpotExceptionsOutOfSpecRepositoryV2>();
            _SpotExceptionsOutOfSpecServiceV2Mock = new Mock<ISpotExceptionsOutOfSpecServiceV2>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();

            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _SharedFolderServiceMock=new Mock<ISharedFolderService>();
            _FileServicesMock=new Mock<IFileService>();
            _AabEngine = new Mock<IAabEngine>();
            _GenreCacheMock = new Mock<IGenreCache>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsOutOfSpecRepository>())
                .Returns(_SpotExceptionsOutOfSpecRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsOutOfSpecRepositoryV2>())
                .Returns(_SpotExceptionsOutOfSpecRepositoryV2Mock.Object);


            _DataRepositoryFactoryMock
              .Setup(x => x.GetDataRepository<IPlanRepository>())
              .Returns(_PlanRepositoryMock.Object);

            _SpotExceptionsOutOfSpecService = new SpotExceptionsOutOfSpecServiceV2
                (
                    _DataRepositoryFactoryMock.Object,
                    _FeatureToggleMock.Object,                   
                    _DateTimeEngineMock.Object,                    
                     _FileServicesMock.Object,
                      _SharedFolderServiceMock.Object,
                      _AabEngine.Object,
                       _ConfigurationSettingsHelperMock.Object
                );
        }

        [Test]
        public async Task GetOutOfSpecPlanToDoAsync_V2()
        {
            // Arrange
            var request = new OutOfSpecPlansIncludingFiltersRequestDto
            {
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansToDoAsync(It.IsAny<List<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(_GetSpotExceptionsOutOfSpecGroupings()));

            _AabEngine
                .Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(new AdvertiserDto
                {
                    AgencyId = 1,
                    AgencyMasterId = new Guid("c56972b6-67f2-4986-a2be-4b1f199a1420"),
                    Name = "Lakme"
                });
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(SpotExceptionsOutOfSpecGroupingToDoResults), "SyncedTimestamp");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetOutOfSpecPlansTodoAsync(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        public void GetOutOfSpecPlanToDoAsync_V2_ThrowException()
        {
            // Arrange
            const string exceptionMessage = "Could not retrieve Spot Exceptions Out Of Spec Plan Todo";
            var request = new OutOfSpecPlansIncludingFiltersRequestDto
            {
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansToDoAsync(It.IsAny<List<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                 .Callback(() =>
                 {
                     throw new CadentException(exceptionMessage);
                 });

            _AabEngine
                .Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(new AdvertiserDto
                {
                    AgencyId = 1,
                    AgencyMasterId = new Guid("c56972b6-67f2-4986-a2be-4b1f199a1420"),
                    Name = "Lakme"
                });
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(SpotExceptionsOutOfSpecGroupingToDoResults), "SyncedTimestamp");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            // Act            
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetOutOfSpecPlansTodoAsync(request));

            // Assert
            Assert.AreEqual(exceptionMessage, result.Message);
        }

        [Test]
        public async Task GetOutOfSpecPlanInventorySourcesAsync_Exist()
        {
            // Arrange
            var request = new OutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            List<string> outOfSpecToDo = new List<string> { "TBA", "Ference Media" };
            List<string> outOfSpecDone = new List<string> { "TVB", "Ference Media" };

            int expectedFerenceMediaCount = 3;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanToDoInventorySourcesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanDoneInventorySourcesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetOutOfSpecPlanInventorySourcesAsync(request);

            // Assert
            Assert.AreEqual(result.Count, expectedFerenceMediaCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async Task GetOutOfSpecPlanInventorySourcesAsync_DoesNotExist()
        {
            // Arrange
            var request = new OutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            List<string> outOfSpecToDo = new List<string> { };
            List<string> outOfSpecDone = new List<string>();

            int expectedCount = 0;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanToDoInventorySourcesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanDoneInventorySourcesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetOutOfSpecPlanInventorySourcesAsync(request);

            // Assert
            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void GetOutOfSpecPlanInventorySourcesAsync_ThrowsException()
        {
            // Arrange
            var request = new OutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = new List<string> { "TBA" };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanToDoInventorySourcesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanDoneInventorySourcesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetOutOfSpecPlanInventorySourcesAsync(request));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Plan Inventory Sources V2", result.Message);
        }

        [Test]
        public async Task GetOutOfSpecSpotInventorySourcesAsync_Exist()
        {
            // Arrange
            var request = new OutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            List<string> outOfSpecToDo = new List<string> { "TBA", "Ference Media" };
            List<string> outOfSpecDone = new List<string> { "TVB", "Ference Media" };

            int expectedFerenceMediaCount = 2;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotToDoInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotDoneInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetOutOfSpecSpotInventorySourcesAsync(request);

            // Assert
            Assert.AreEqual(result[0].Count, expectedFerenceMediaCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async Task GetSpotExceptionsOutOfSpecInventorySources_V2_InventorySource_DoesNotExist()
        {
            // Arrange
            var request = new OutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            List<string> outOfSpecToDo = new List<string> { };
            List<string> outOfSpecDone = new List<string>();

            int expectedCount = 0;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotToDoInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotDoneInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecDone));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetOutOfSpecSpotInventorySourcesAsync(request);

            // Assert
            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecInventorySources_ThrowsException()
        {
            // Arrange
            var request = new OutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = new List<string> { "TBA" };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotToDoInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(outOfSpecToDo));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotDoneInventorySourcesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetOutOfSpecSpotInventorySourcesAsync(request));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Inventory Sources V2", result.Message);
        }

        [Test]
        public async Task GetOutOfSpecSpotReasonCodesAsync_Exist()
        {
            // Arrange
            var request = new OutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotToDoReasonCodesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(new List<OutOfSpecSpotReasonCodesDto>
                {
                    new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart",
                        Count = 10
                    },
                    new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 3,
                        ReasonCode = 2,
                        Reason = "genre content restriction",
                        Label = "Genre",
                        Count = 20
                    },
                    new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate",
                        Count = 30
                    }
                }));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
               .Setup(s => s.GetOutOfSpecSpotDoneReasonCodesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
               .Returns(Task.FromResult(new List<OutOfSpecSpotReasonCodesDto>
               {
                    new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart",
                        Count = 5
                    },
                    new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate",
                        Count = 10
                    }
               }));

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetOutOfSpecSpotReasonCodesAsync(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public async Task GetOutOfSpecSpotReasonCodesAsync_DoNotExist()
        {
            // Arrange
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotToDoReasonCodesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(new List<OutOfSpecSpotReasonCodesDto>()));

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotDoneReasonCodesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(new List<OutOfSpecSpotReasonCodesDto>()));

            var request = new OutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetOutOfSpecSpotReasonCodesAsync(request);

            // Assert
            Assert.AreEqual(0, result.Count);
        }      

        [Test]
        public void GetOutOfSpecSpotReasonCodesAsync_ThrowsException()
        {
            // Arrange
            var request = new OutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotToDoReasonCodesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotDoneReasonCodesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetOutOfSpecSpotReasonCodesAsync(request));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Spot Reason Codes V2", result.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async void GetOutOfSpec_DonePlansInventorySourceFilter_WithoutExist()
        {
            // Arrange
            OutOfSpecPlansIncludingFiltersRequestDto spotExceptionsOutofSpecsPlansIncludingFiltersRequest = new OutOfSpecPlansIncludingFiltersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10),
                InventorySourceNames = new List<string> { }
            };

            var outOfSpecDone = new List<SpotExceptionsOutOfSpecGroupingDto>();

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetOutOfSpecPlansDoneAsync(spotExceptionsOutofSpecsPlansIncludingFiltersRequest);

            // Assert
            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async void GetOutOfSpec_DonePlansInventorySourceFilter_Exist()
        {
            // Arrange
            OutOfSpecPlansIncludingFiltersRequestDto spotExceptionsOutofSpecsPlansIncludingFiltersRequest = new OutOfSpecPlansIncludingFiltersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10),
                InventorySourceNames = new List<string> { "Ference POD" }
            };

            var outOfSpecDone = _GetOutOfSpecGroupingDoneData();

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
                .Returns(Task.FromResult(outOfSpecDone));

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = await _SpotExceptionsOutOfSpecService.GetOutOfSpecPlansDoneAsync(spotExceptionsOutofSpecsPlansIncludingFiltersRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(result.Count, 1);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetOutOfSpec_DonePlansInventorySourceFilter_ThrowException()
        {
            // Arrange
            OutOfSpecPlansIncludingFiltersRequestDto spotExceptionsOutofSpecsPlansIncludingFilterRequest = new OutOfSpecPlansIncludingFiltersRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10),
                InventorySourceNames = new List<string> { }
            };

            var outOfSpecDone = new List<SpotExceptionsOutOfSpecGroupingDto>();

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansDoneAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            _AabEngine.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = Assert.Throws<CadentException>(async () => await _SpotExceptionsOutOfSpecService.GetOutOfSpecPlansDoneAsync(spotExceptionsOutofSpecsPlansIncludingFilterRequest)); ;

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Plans", result.Message);
        }

        private List<SpotExceptionsOutOfSpecGroupingDto> _GetOutOfSpecGroupingDoneData()
        {
            return new List<SpotExceptionsOutOfSpecGroupingDto>()
            {
                new SpotExceptionsOutOfSpecGroupingDto
                {
                    PlanId = 334,
                    AdvertiserMasterId = new Guid( "C56972B6-67F2-4986-A2BE-4B1F199A1420" ),
                    PlanName = "4Q'21 Macy's SYN",
                    AffectedSpotsCount = 1,
                    Impressions = 10,
                    FlightStartDate = new DateTime(2021, 9, 28),
                    FlightEndDate = new DateTime(2021, 12, 06),
                    SpotLengths = new List<SpotLengthDto>
                    {
                        new SpotLengthDto
                        {
                            Length = 15,
                        }
                    },
                    AudienceName = "Women 25-54"
                }
            };
        }

        private List<SpotExceptionsOutOfSpecGroupingDto> _GetSpotExceptionsOutOfSpecGroupings()
        {
            return new List<SpotExceptionsOutOfSpecGroupingDto>
            {
                new SpotExceptionsOutOfSpecGroupingDto
                {
                    AdvertiserMasterId = new Guid("c56972b6-67f2-4986-a2be-4b1f199a1420"),
                    AffectedSpotsCount = 1,
                    AudienceName = "Women 25-54",
                    FlightStartDate = new DateTime(2022,10,01),
                    FlightEndDate = new DateTime(2022,12,30),
                    Impressions = 0.0,
                     PlanId = 334,
                     PlanName= "4Q'21 Macy's SYN",
                      SpotLengths = new List<SpotLengthDto>
                      {
                          new SpotLengthDto
                          {
                               Id=1,
                               Length = 15
                          },
                           new SpotLengthDto
                          {
                               Id=2,
                               Length = 30
                          },
                      },
                       SpotLengthString = ":15"
                },
                new SpotExceptionsOutOfSpecGroupingDto
                {
                    AdvertiserMasterId = new Guid("c56972b6-67f2-4986-a2be-4b1f199a1420"),
                    AffectedSpotsCount = 1,
                    AudienceName = "Women 25-40",
                    FlightStartDate = new DateTime(2022,11,01),
                    FlightEndDate = new DateTime(2022,12,30),
                    Impressions = 0.0,
                     PlanId = 335,
                     PlanName= "5Q'21 Macy's SYN",
                      SpotLengths = new List<SpotLengthDto>
                      {
                          new SpotLengthDto
                          {
                               Id=1,
                               Length = 15
                          },
                           new SpotLengthDto
                          {
                               Id=2,
                               Length = 30
                          },
                      },
                       SpotLengthString = ":15"
                }
            };
        }
    }
}
