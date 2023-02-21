using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
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
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using ApprovalTests;
using ApprovalTests.Reporters;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.SpotExceptions
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionsOutOfSpecServiceUnitTestsV2
    {
        private SpotExceptionsOutOfSpecServiceV2 _SpotExceptionsOutOfSpecServiceV2;

        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISpotExceptionsOutOfSpecRepositoryV2> _SpotExceptionsOutOfSpecRepositoryV2Mock;
        private Mock<IPlanRepository> _PlanRepositoryMock;

        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IGenreCache> _GenreCacheMock;
        private Mock<IAabEngine> _AabEngineMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private Mock<IFileService> _FileServicesMock;
        private Mock<ISharedFolderService> _SharedFolderServiceMock;
       
        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotExceptionsOutOfSpecRepositoryV2Mock = new Mock<ISpotExceptionsOutOfSpecRepositoryV2>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();

            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _SharedFolderServiceMock=new Mock<ISharedFolderService>();
            _FileServicesMock=new Mock<IFileService>();
            _GenreCacheMock = new Mock<IGenreCache>();
            _AabEngineMock = new Mock<IAabEngine>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionsOutOfSpecRepositoryV2>())
                .Returns(_SpotExceptionsOutOfSpecRepositoryV2Mock.Object);

            _DataRepositoryFactoryMock
              .Setup(x => x.GetDataRepository<IPlanRepository>())
              .Returns(_PlanRepositoryMock.Object);

            _SpotExceptionsOutOfSpecServiceV2 = new SpotExceptionsOutOfSpecServiceV2
                (
                    _DataRepositoryFactoryMock.Object,
                    _FeatureToggleMock.Object,                   
                    _DateTimeEngineMock.Object,                    
                    _FileServicesMock.Object,
                    _SharedFolderServiceMock.Object,
                    _GenreCacheMock.Object,
                    _AabEngineMock.Object,
                    _ConfigurationSettingsHelperMock.Object
                );
        }

        [Test]
        public void GetOutOfSpecPlansToDo_Exist()
        {
            // Arrange
            var request = new OutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansToDo(It.IsAny<List<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetOutOfSpecPlansToDo());

            _AabEngineMock
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
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecPlansToDo(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        public void GetOutOfSpecPlansToDo_DoesNotExist()
        {
            // Arrange
            var request = new OutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };
            var outOfSpecToDo = new List<OutOfSpecPlansDto>();

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansToDo(It.IsAny<List<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            _AabEngineMock
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
            var result =  _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecPlansToDo(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }

        [Test]
        public void GetOutOfSpecPlansToDo_ThrowException()
        {
            // Arrange
            const string exceptionMessage = "Could not retrieve Spot Exceptions Out Of Spec Plans Todo";
            var request = new OutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansToDo(It.IsAny<List<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                 .Callback(() =>
                 {
                     throw new CadentException(exceptionMessage);
                 });

            _AabEngineMock
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
            var result = Assert.Throws<CadentException>(() => _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecPlansToDo(request));

            // Assert
            Assert.AreEqual(exceptionMessage, result.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetOutOfSpecPlansDone_Exist()
        {
            // Arrange
            OutOfSpecPlansRequestDto outOfSpecPlansRequest = new OutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10),
                InventorySourceNames = new List<string> { "Ference POD" }
            };

            var outOfSpecDone = _GetOutOfSpecPlansDone();

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansDone(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
                .Returns(outOfSpecDone);

            _AabEngineMock.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecPlansDone(outOfSpecPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetOutOfSpecPlansDone_DoesNotExist()
        {
            // Arrange
            OutOfSpecPlansRequestDto outOfSpecPlansRequest = new OutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10),
                InventorySourceNames = new List<string> { }
            };

            var outOfSpecDone = new List<OutOfSpecPlansDto>();

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansDone(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
                .Returns(outOfSpecDone);

            _AabEngineMock.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecPlansDone(outOfSpecPlansRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetOutOfSpecPlansDone_ThrowException()
        {
            // Arrange
            OutOfSpecPlansRequestDto spotExceptionsOutofSpecsPlansIncludingFilterRequest = new OutOfSpecPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10),
                InventorySourceNames = new List<string> { }
            };

            var outOfSpecDone = new List<SpotExceptionsOutOfSpecGroupingDto>();

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlansDone(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            _AabEngineMock.Setup(s => s.GetAdvertiser(It.IsAny<Guid>()))
                .Returns<Guid>(g => new AdvertiserDto { Name = $"Advertiser With Id ='{g}'" });

            // Act
            var result = Assert.Throws<CadentException>(() => _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecPlansDone(spotExceptionsOutofSpecsPlansIncludingFilterRequest)); ;

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Plans Done", result.Message);
        }

        [Test]
        public void GetOutOfSpecPlanInventorySources_Exist()
        {
            // Arrange
            var request = new OutOfSpecPlanInventorySourcesRequestDto
            {
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            List<string> outOfSpecToDo = new List<string> { "TBA", "Ference Media" };
            List<string> outOfSpecDone = new List<string> { "TVB", "Ference Media" };

            int expectedFerenceMediaCount = 3;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanToDoInventorySources(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanDoneInventorySources(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecDone);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecPlanInventorySources(request);

            // Assert
            Assert.AreEqual(result.Count, expectedFerenceMediaCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetOutOfSpecPlanInventorySourcesAsync_DoesNotExist()
        {
            // Arrange
            var request = new OutOfSpecPlanInventorySourcesRequestDto
            {
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            List<string> outOfSpecToDo = new List<string> { };
            List<string> outOfSpecDone = new List<string>();

            int expectedCount = 0;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanToDoInventorySources(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanDoneInventorySources(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecDone);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecPlanInventorySources(request);

            // Assert
            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void GetOutOfSpecPlanInventorySourcesAsync_ThrowsException()
        {
            // Arrange
            var request = new OutOfSpecPlanInventorySourcesRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = new List<string> { "TBA" };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanToDoInventorySources(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecPlanDoneInventorySources(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(() => _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecPlanInventorySources(request));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Plan Inventory Sources", result.Message);
        }

        [Test]
        public void SaveOutOfSpecPlanAcceptance_SinglePlanId()
        {
            // Arrange
            var inventorySourceNames = new List<string>();
            var planIds = new List<int>() { 215 };
            string userName = "Test User";
            bool expectedResult = true;
            var outOfSpecToDo = _GetOutOfSpecPlanSpotsData();

            var saveOutOfSpecPlanAcceptanceRequest = new SaveOutOfSpecPlanAcceptanceRequestDto
            {
                PlanIds = planIds,
                Filters = new OutOfSpecPlansRequestDto()
                {
                    InventorySourceNames = inventorySourceNames,
                    WeekStartDate = new DateTime(2021, 01, 04),
                    WeekEndDate = new DateTime(2021, 01, 10)
                }
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecPlanAcceptance(saveOutOfSpecPlanAcceptanceRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SaveOutOfSpecPlanAcceptance_MultiplePlanIds()
        {
            // Arrange
            var inventorySourceNames = new List<string>();
            var planIds = new List<int>() { 215, 674 };
            string userName = "Test User";
            bool expectedResult = true;
            var outOfSpecToDo = _GetOutOfSpecPlanSpotsData();

            var saveOutOfSpecPlanAcceptanceRequest = new SaveOutOfSpecPlanAcceptanceRequestDto
            {
                PlanIds = planIds,
                Filters = new OutOfSpecPlansRequestDto()
                {
                    InventorySourceNames = inventorySourceNames,
                    WeekStartDate = new DateTime(2021, 01, 04),
                    WeekEndDate = new DateTime(2021, 01, 10)
                }
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecPlanAcceptance(saveOutOfSpecPlanAcceptanceRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SaveOutOfSpecPlanAcceptance_SinglePlanIdWithInventorySource()
        {
            // Arrange
            var inventorySourceNames = new List<string>() { "TVB" };
            var planIds = new List<int>() { 215 };
            string userName = "Test User";
            bool expectedResult = true;
            var outOfSpecToDo = _GetOutOfSpecPlanSpotsData();

            var saveOutOfSpecPlanAcceptanceRequest = new SaveOutOfSpecPlanAcceptanceRequestDto
            {
                PlanIds = planIds,
                Filters = new OutOfSpecPlansRequestDto()
                {
                    InventorySourceNames = inventorySourceNames,
                    WeekStartDate = new DateTime(2021, 01, 04),
                    WeekEndDate = new DateTime(2021, 01, 10)
                }
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);
            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecPlanAcceptance(saveOutOfSpecPlanAcceptanceRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SaveOutOfSpecPlanAcceptance_MultiplePlanIdsWithInventorySource()
        {
            // Arrange
            var inventorySourceNames = new List<string>() { "TVB" };
            var planIds = new List<int>() { 215, 674 };
            string userName = "Test User";
            bool expectedResult = true;
            var outOfSpecToDo = _GetOutOfSpecPlanSpotsData();

            var saveOutOfSpecPlanAcceptanceRequest = new SaveOutOfSpecPlanAcceptanceRequestDto
            {
                PlanIds = planIds,
                Filters = new OutOfSpecPlansRequestDto()
                {
                    InventorySourceNames = inventorySourceNames,
                    WeekStartDate = new DateTime(2021, 01, 04),
                    WeekEndDate = new DateTime(2021, 01, 10)
                }
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);
            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecPlanAcceptance(saveOutOfSpecPlanAcceptanceRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SaveOutOfSpecPlanAcceptance_ThrowsException()
        {
            // Arrange
            var inventorySourceNames = new List<string>();
            var planIds = new List<int>() { 215 };
            string userName = "Test User";
            var outOfSpecToDo = _GetOutOfSpecPlanSpotsData();

            var saveOutOfSpecPlanAcceptanceRequest = new SaveOutOfSpecPlanAcceptanceRequestDto
            {
                PlanIds = planIds,
                Filters = new OutOfSpecPlansRequestDto()
                {
                    InventorySourceNames = inventorySourceNames,
                    WeekStartDate = new DateTime(2021, 01, 04),
                    WeekEndDate = new DateTime(2021, 01, 10)
                }
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(() => _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecPlanAcceptance(saveOutOfSpecPlanAcceptanceRequest, userName));

            // Assert
            Assert.AreEqual("Could Not Save The Out Of Spec Plan Through Acceptance", result.Message);
        }

        [Test]
        public void GetOutOfSpecSpotInventorySources_Exist()
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
                .Setup(x => x.GetOutOfSpecSpotToDoInventorySources(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotDoneInventorySources(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecDone);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotInventorySources(request);

            // Assert
            Assert.AreEqual(result[0].Count, expectedFerenceMediaCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetOutOfSpecSpotInventorySources_DoesNotExist()
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
                .Setup(x => x.GetOutOfSpecSpotToDoInventorySources(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotDoneInventorySources(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecDone);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotInventorySources(request);

            // Assert
            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public void GetOutOfSpecSpotInventorySources_ThrowsException()
        {
            // Arrange
            var request = new OutOfSpecSpotsRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            var outOfSpecToDo = new List<string> { "TBA" };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotToDoInventorySources(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotDoneInventorySources(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(() => _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotInventorySources(request));

            // Assert
            Assert.AreEqual("Could Not retrieve Out Of Spec Spot Inventory Sources", result.Message);
        }

        [Test]
        public void GetOutOfSpecSpotReasonCodes_Exist()
        {
            // Arrange
            var request = new OutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotToDoReasonCodes(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<OutOfSpecSpotReasonCodesDto>
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
                });

            _SpotExceptionsOutOfSpecRepositoryV2Mock
               .Setup(s => s.GetOutOfSpecSpotDoneReasonCodes(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
               .Returns(new List<OutOfSpecSpotReasonCodesDto>
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
               });

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotReasonCodes(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetOutOfSpecSpotReasonCodes_DoNotExist()
        {
            // Arrange
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotToDoReasonCodes(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<OutOfSpecSpotReasonCodesDto>());

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotDoneReasonCodes(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<OutOfSpecSpotReasonCodesDto>());

            var request = new OutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotReasonCodes(request);

            // Assert
            Assert.AreEqual(0, result.Count);
        }      

        [Test]
        public void GetOutOfSpecSpotReasonCodes_ThrowsException()
        {
            // Arrange
            var request = new OutOfSpecSpotsRequestDto
            {
                PlanId = 847,
                WeekStartDate = new DateTime(2022, 01, 01),
                WeekEndDate = new DateTime(2022, 12, 31)
            };
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotToDoReasonCodes(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotDoneReasonCodes(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(() => _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotReasonCodes(request));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Spot Reason Codes", result.Message);
        }             

        [Test]
        public void GetOutOfSpecSpotPrograms_ExistInProgramsOnly()
        {
            // Arrange
            string programNameQuery = "WNRUSH";

            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromPrograms(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto>
                    {
                        new ProgramNameDto
                        {
                           OfficialProgramName = "WNRUSH",
                           GenreId=  33
                        }
                    }
                );

            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromSpotExceptionDecisions(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto> { });

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                    Id = 33,
                    Display = "Genre"
                });

            // Act           
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotPrograms(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetOutOfSpecSpotPrograms_ExistInDecisionsOnly()
        {
            // Arrange
            string programNameQuery = "WNRUSH";

            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromPrograms(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto> { });

            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromSpotExceptionDecisions(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto>
                    {
                        new ProgramNameDto
                        {
                           OfficialProgramName = "#WNRUSH",
                           GenreId = 33
                        }
                    }
                );

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                    Id = 33,
                    Display = "Genre"
                });

            // Act           
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotPrograms(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetOutOfSpecSpotPrograms_ExistInProgramsOnly_NoGenre()
        {
            // Arrange
            string programNameQuery = "WNRUSH";

            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromPrograms(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto>
                    {
                        new ProgramNameDto
                        {
                           OfficialProgramName = "WNRUSH",
                           GenreId = null
                        }
                    }
                );

            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromSpotExceptionDecisions(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto> { });

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto { });

            // Act           
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotPrograms(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetOutOfSpecSpotPrograms_ExistInDecisionsOnly_NoGenre()
        {
            // Arrange
            string programNameQuery = "WNRUSH";

            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromPrograms(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto> { });

            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromSpotExceptionDecisions(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto>
                    {
                        new ProgramNameDto
                        {
                           OfficialProgramName = "#WNRUSH",
                           GenreId = null
                        }
                    }
                );

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto{});

            // Act           
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotPrograms(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetOutOfSpecSpotPrograms_DoesNotExist()
        {
            // Arrange
            string programNameQuery = "WNRUSH";
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromPrograms(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto> { });

            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromSpotExceptionDecisions(It.IsAny<string>()))
                .Returns(new List<ProgramNameDto> { });

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                    Id = 33,
                    Display = "Genre"
                });

            // Act           
            var result = _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotPrograms(programNameQuery, "TestsUser");

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetOutOfSpecSpotPrograms_ThrowsException()
        {
            // Arrange
            string programNameQuery = "WNRUSH";
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Setup(s => s.FindProgramFromPrograms(It.IsAny<string>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            _GenreCacheMock
                .Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
                .Returns(new LookupDto
                {
                    Id = 1,
                    Display = "Genre"
                });

            // Act           
            var result = Assert.Throws<CadentException>(() => _SpotExceptionsOutOfSpecServiceV2.GetOutOfSpecSpotPrograms(programNameQuery, "TestsUser"));

            // Assert
            Assert.AreEqual("Could not retrieve Spot Exceptions Out Of Spec Spot Programs", result.Message);
        }

        [Test]
        public void SaveOutOfSpecSpotCommentsToDo_AddSingleCommentToDo()
        {
            // Arrange
            string userName = "Unit Test";
            var outOfSpecToDo = _GetOutOfSpecPlanSpotSingleAddCommentData();
            var commentsAddedCount = 1;

            var saveOutOfSpecSpotCommentsRequest = new SaveOutOfSpecSpotCommentsRequestDto()
            {
                SpotIds = new List<int> { 14 },
                Comment = "Unit Test Add Single Comment"
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Returns(commentsAddedCount);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotCommentsToDo(saveOutOfSpecSpotCommentsRequest, userName);

            // Assert
            Assert.IsTrue(result);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Never);
        }

        [Test]
        public void SaveOutOfSpecSpotCommentsToDo_AddMultipleCommentsToDo()
        {
            // Arrange
            string userName = "Unit Test";
            var outOfSpecToDo = _GetOutOfSpecPlanSpotMultipleAddCommentData();
            var commentsAddedCount = 2;

            var saveOutOfSpecSpotCommentsRequest = new SaveOutOfSpecSpotCommentsRequestDto()
            {
                SpotIds = new List<int> { 14, 15 },
                Comment = "Unit Test Add Multiple Comment"
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Returns(commentsAddedCount);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotCommentsToDo(saveOutOfSpecSpotCommentsRequest, userName);

            // Assert
            Assert.IsTrue(result);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Never);
        }

        [Test]
        public void SaveOutOfSpecSpotCommentsToDo_UpdateSingleCommentToDo()
        {
            // Arrange
            string userName = "Unit Test";
            var outOfSpecToDo = _GetOutOfSpecPlanSpotSingleUpdateCommentData();
            var commentsUpdatedCount = 1;

            var saveOutOfSpecSpotCommentsRequest = new SaveOutOfSpecSpotCommentsRequestDto()
            {
                SpotIds = new List<int> { 3 },
                Comment = "Unit Test Add Multiple Comment"
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Returns(commentsUpdatedCount);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotCommentsToDo(saveOutOfSpecSpotCommentsRequest, userName);

            // Assert
            Assert.IsTrue(result);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Never);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Once);
        }

        [Test]
        public void SaveOutOfSpecSpotCommentsToDo_UpdateMultipleCommentsToDo()
        {
            // Arrange
            string userName = "Unit Test";
            var outOfSpecToDo = _GetOutOfSpecPlanSpotMultipleUpdateCommentData();
            var commentsUpdatedCount = 2;

            var saveOutOfSpecSpotCommentsRequest = new SaveOutOfSpecSpotCommentsRequestDto()
            {
                SpotIds = new List<int> { 3, 4 },
                Comment = "Unit Test Add Multiple Comment"
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Returns(commentsUpdatedCount);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotCommentsToDo(saveOutOfSpecSpotCommentsRequest, userName);

            // Assert
            Assert.IsTrue(result);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Never);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Once);
        }

        [Test]
        public void SaveOutOfSpecSpotCommentsToDo_AddUpdateSingleCommentToDo()
        {
            // Arrange
            string userName = "Unit Test";
            var outOfSpecToDo = _GetOutOfSpecPlanSpotSingleAddUpdateCommentData();
            var commentsAddedCount = 1;
            var commentsUpdatedCount = 1;

            var saveOutOfSpecSpotCommentsRequest = new SaveOutOfSpecSpotCommentsRequestDto()
            {
                SpotIds = new List<int> { 3, 14 },
                Comment = "Unit Test Add Multiple Comment"
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Returns(commentsAddedCount);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Returns(commentsUpdatedCount);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotCommentsToDo(saveOutOfSpecSpotCommentsRequest, userName);

            // Assert
            Assert.IsTrue(result);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Once);
        }

        [Test]
        public void SaveOutOfSpecSpotCommentsToDo_AddUpdateMultipleCommentsToDo()
        {
            // Arrange
            string userName = "Unit Test";
            var outOfSpecToDo = _GetOutOfSpecPlanSpotsData();
            var commentsAddedCount = 2;
            var commentsUpdatedCount = 2;

            var saveOutOfSpecSpotCommentsRequest = new SaveOutOfSpecSpotCommentsRequestDto()
            {
                SpotIds = new List<int> { 3, 4, 14, 15 },
                Comment = "Unit Test Add Multiple Comment"
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Returns(commentsAddedCount);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Returns(commentsUpdatedCount);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotCommentsToDo(saveOutOfSpecSpotCommentsRequest, userName);

            // Assert
            Assert.IsTrue(result);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.GetOutOfSpecSpotsToDoByIds(It.IsAny<List<int>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Once);
            _SpotExceptionsOutOfSpecRepositoryV2Mock.Verify(s => s.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()), Times.Once);
        }

        [Test]
        public void SaveOutOfSpecSpotCommentsToDo_ThrowsException()
        {
            // Arrange
            string userName = "Unit Test";
            var outOfSpecToDo = _GetOutOfSpecPlanSpotsData();
            var commentsAddedCount = 1;

            var saveOutOfSpecSpotCommentsRequest = new SaveOutOfSpecSpotCommentsRequestDto()
            {
                SpotIds = new List<int> { 3, 14 },
                Comment = "Unit Test Add Multiple Comment"
            };

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.GetOutOfSpecSpotsToDo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(outOfSpecToDo);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.AddOutOfSpecSpotCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Returns(commentsAddedCount);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(x => x.UpdateOutOfSpecCommentsToDo(It.IsAny<List<OutOfSpecSpotCommentsDto>>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<CadentException>(() => _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotCommentsToDo(saveOutOfSpecSpotCommentsRequest, userName));

            // Assert
            Assert.AreEqual("Could Not Save The Out Of Spec Spot Comments To Do", result.Message);
        }

        [Test]
        public void SaveOutOfSpecSpotsBulkEditDone_SaveSingleDoneComment()
        {
            // Arrange
            List<int> spots = new List<int>() { 1, 2 };
            var outOfSpecSpotBulkEditRequest = new SaveOutOfSpecSpotBulkEditRequestDto
            {
                SpotIds = spots,
                Decisions = new OutOfSpecDecisionsToSaveRequestDto
                {
                    AcceptAsInSpec = true,
                    Comments = "Unit test comments"
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.SaveOutOfSpecSpotComments(It.IsAny<List<OutOfSpecSpotCommentsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(expectedResult);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotsDoneByIds(It.IsAny<List<int>>()))
                .Returns(_GetOutOfSpecDoneData());

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotsBulkEditDone(outOfSpecSpotBulkEditRequest, userName);


            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SaveOutOfSpecSpotsBulkEditDone_SaveCommentAndDecision()
        {
            List<int> spots = new List<int>() { 1, 2 };
            // Arrange
            var outOfSpecSpotBulkEditRequest = new SaveOutOfSpecSpotBulkEditRequestDto
            {
                SpotIds = spots,
                Decisions = new OutOfSpecDecisionsToSaveRequestDto
                {
                    AcceptAsInSpec = true,
                    Comments = "Unit test comments"
                }
            };

            string userName = "Test User";
            bool expectedResult = true;

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.SaveOutOfSpecSpotComments(It.IsAny<List<OutOfSpecSpotCommentsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(expectedResult);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotsDoneByIds(It.IsAny<List<int>>()))
               .Returns(_GetOutOfSpecDoneData());

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.SaveOutOfSpecSpotDoneDecisions(It.IsAny<List<OutOfSpecSpotDoneDecisionsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(expectedResult);

            // Act
            var result = _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotsBulkEditDone(outOfSpecSpotBulkEditRequest, userName);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SaveOutOfSpecSpotsBulkEditDone_SaveSingleDoneComment_ThrowException()
        {
            // Arrange
            List<int> spots = new List<int>() { 1, 2 };
            var outOfSpecSpotBulkEditRequest = new SaveOutOfSpecSpotBulkEditRequestDto
            {
                SpotIds = spots,
                Decisions = new OutOfSpecDecisionsToSaveRequestDto
                {
                    AcceptAsInSpec = true,
                    Comments = "Unit test comments"
                }
            };

            string userName = "Test User";
            string expectedResult = "Could not save Decisions to Out of Spec";

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.SaveOutOfSpecSpotComments(It.IsAny<List<OutOfSpecSpotCommentsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(true);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotsDoneByIds(It.IsAny<List<int>>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(() =>
            _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotsBulkEditDone(outOfSpecSpotBulkEditRequest, userName));

            // Assert
            Assert.AreEqual(expectedResult, result.Message);
        }

        [Test]
        public void SaveOutOfSpecSpotsBulkEditDone_ThrowException()
        {
            // Arrange
            List<int> spots = new List<int>() { 1, 2 };
            var outOfSpecSpotBulkEditRequest = new SaveOutOfSpecSpotBulkEditRequestDto
            {
                SpotIds = spots,
                Decisions = new OutOfSpecDecisionsToSaveRequestDto
                {
                    AcceptAsInSpec = true,
                    Comments = "Unit test comments"
                }
            };

            string userName = "Test User";
            string expectedResult = "Could not save Decisions to Out of Spec";

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.SaveOutOfSpecSpotComments(It.IsAny<List<OutOfSpecSpotCommentsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(true);

            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.GetOutOfSpecSpotsDoneByIds(It.IsAny<List<int>>()))
                .Returns(_GetOutOfSpecDoneData());
            _SpotExceptionsOutOfSpecRepositoryV2Mock
                .Setup(s => s.SaveOutOfSpecSpotDoneDecisions(It.IsAny<List<OutOfSpecSpotDoneDecisionsDto>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new CadentException("Throwing a test exception.");
                });

            // Act           
            var result = Assert.Throws<CadentException>(() =>
            _SpotExceptionsOutOfSpecServiceV2.SaveOutOfSpecSpotsBulkEditDone(outOfSpecSpotBulkEditRequest, userName));

            // Assert
            Assert.AreEqual(expectedResult, result.Message);
        }

        /// <summary>
        /// Gets the out of spec plans done.
        /// </summary>
        /// <returns></returns>
        private List<OutOfSpecPlansDto> _GetOutOfSpecPlansDone()
        {
            return new List<OutOfSpecPlansDto>()
            {
                new OutOfSpecPlansDto
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

        private List<OutOfSpecSpotsToDoDto> _GetOutOfSpecPlanSpotSingleAddCommentData()
        {
            return new List<OutOfSpecSpotsToDoDto>()
            {                
                new OutOfSpecSpotsToDoDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY1MzI1MjM=",
                    ExecutionIdExternal = "2212161638441z7VnXU_R3",
                    ReasonCodeMessage = null,
                    EstimateId = 2009,
                    IsciName = "JARD0075000H",
                    RecommendedPlanId = 674,
                    RecommendedPlanName = "Boehringer Jardiance 4Q22 BYU EM News",
                    ProgramName = "BUSINESS FIRST AM",
                    StationLegacyCallLetters = "KOLD",
                    Affiliate= "CBS",
                    Market = "Tucson (Sierra Vista)",
                    AdvertiserMasterId = new Guid("1d0fa038-6a70-4907-b9ba-739ab67e35ad"),
                    AdvertiserName = null,
                    SpotLengthId = null,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 2,
                        Length= 60
                    },
                    AudienceId = null,
                    Audience = new AudienceDto
                    {
                        Id = 40,
                        Code = "A35-64",
                        Name = "Adults 35-64"
                    },
                    Product = null,
                    FlightStartDate = new DateTime(2022, 12, 12),
                    FlightEndDate = new DateTime(2022, 12, 25),
                    DaypartCode = "EMN",
                    GenreName = "INFORMATIONAL/NEWS",
                    DaypartDetail =
                    {
                        Id = 0,
                        Code = null,
                        Name = null,
                        DaypartText = null
                    },
                    ProgramNetwork = "CBS",
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    IngestedBy = "Test User",
                    IngestedAt = new DateTime(2022, 12, 12),
                    Impressions = 464.37199999999996,
                    IngestedMediaWeekId = 989,
                    PlanId = 674,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Description = null,
                        Label = "Time"
                    },
                    MarketCode = 289,
                    MarketRank = 69,
                    HouseIsci = "009ARD0075H",
                    TimeZone = "EST",
                    DMA = 58,
                    Comment = null,
                    InventorySourceName = "Business First AM"
                }
            };
        }

        private List<OutOfSpecSpotsToDoDto> _GetOutOfSpecPlanSpotMultipleAddCommentData()
        {
            return new List<OutOfSpecSpotsToDoDto>()
            {
                new OutOfSpecSpotsToDoDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY1MzI1MjM=",
                    ExecutionIdExternal = "2212161638441z7VnXU_R3",
                    ReasonCodeMessage = null,
                    EstimateId = 2009,
                    IsciName = "JARD0075000H",
                    RecommendedPlanId = 674,
                    RecommendedPlanName = "Boehringer Jardiance 4Q22 BYU EM News",
                    ProgramName = "BUSINESS FIRST AM",
                    StationLegacyCallLetters = "KOLD",
                    Affiliate= "CBS",
                    Market = "Tucson (Sierra Vista)",
                    AdvertiserMasterId = new Guid("1d0fa038-6a70-4907-b9ba-739ab67e35ad"),
                    AdvertiserName = null,
                    SpotLengthId = null,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 2,
                        Length= 60
                    },
                    AudienceId = null,
                    Audience = new AudienceDto
                    {
                        Id = 40,
                        Code = "A35-64",
                        Name = "Adults 35-64"
                    },
                    Product = null,
                    FlightStartDate = new DateTime(2022, 12, 12),
                    FlightEndDate = new DateTime(2022, 12, 25),
                    DaypartCode = "EMN",
                    GenreName = "INFORMATIONAL/NEWS",
                    DaypartDetail =
                    {
                        Id = 0,
                        Code = null,
                        Name = null,
                        DaypartText = null
                    },
                    ProgramNetwork = "CBS",
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    IngestedBy = "Test User",
                    IngestedAt = new DateTime(2022, 12, 12),
                    Impressions = 464.37199999999996,
                    IngestedMediaWeekId = 989,
                    PlanId = 674,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Description = null,
                        Label = "Time"
                    },
                    MarketCode = 289,
                    MarketRank = 69,
                    HouseIsci = "009ARD0075H",
                    TimeZone = "EST",
                    DMA = 58,
                    Comment = null,
                    InventorySourceName = "Business First AM"
                },
                new OutOfSpecSpotsToDoDto
                {
                    Id = 15,
                    SpotUniqueHashExternal = "TE9DQUwtNDY1MzI1MjM=",
                    ExecutionIdExternal = "2212161638441z7VnXU_R3",
                    ReasonCodeMessage = null,
                    EstimateId = 2009,
                    IsciName = "JARD0075000H",
                    RecommendedPlanId = 674,
                    RecommendedPlanName = "Boehringer Jardiance 4Q22 BYU EM News",
                    ProgramName = "BUSINESS FIRST AM",
                    StationLegacyCallLetters = "KOLD",
                    Affiliate= "CBS",
                    Market = "Tucson (Sierra Vista)",
                    AdvertiserMasterId = new Guid("1d0fa038-6a70-4907-b9ba-739ab67e35ad"),
                    AdvertiserName = null,
                    SpotLengthId = null,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 2,
                        Length= 60
                    },
                    AudienceId = null,
                    Audience = new AudienceDto
                    {
                        Id = 40,
                        Code = "A35-64",
                        Name = "Adults 35-64"
                    },
                    Product = null,
                    FlightStartDate = new DateTime(2022, 12, 12),
                    FlightEndDate = new DateTime(2022, 12, 25),
                    DaypartCode = "EMN",
                    GenreName = "INFORMATIONAL/NEWS",
                    DaypartDetail =
                    {
                        Id = 0,
                        Code = null,
                        Name = null,
                        DaypartText = null
                    },
                    ProgramNetwork = "CBS",
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    IngestedBy = "Test User",
                    IngestedAt = new DateTime(2022, 12, 12),
                    Impressions = 464.37199999999996,
                    IngestedMediaWeekId = 989,
                    PlanId = 674,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Description = null,
                        Label = "Time"
                    },
                    MarketCode = 289,
                    MarketRank = 69,
                    HouseIsci = "009ARD0075H",
                    TimeZone = "EST",
                    DMA = 58,
                    Comment = null,
                    InventorySourceName = "Business First AM"
                }
            };
        }

        private List<OutOfSpecSpotsToDoDto> _GetOutOfSpecPlanSpotSingleUpdateCommentData()
        {
            return new List<OutOfSpecSpotsToDoDto>()
            {
                new OutOfSpecSpotsToDoDto
                {
                    Id = 3,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    PlanId = 215,
                     SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                    HouseIsci = "289J76GN16H",
                    Comment = "test Comment",
                    GenreName="Comedy",
                    DaypartCode="ROSP",
                    InventorySourceName = "TVB"
                }
            };
        }

        private List<OutOfSpecSpotsToDoDto> _GetOutOfSpecPlanSpotMultipleUpdateCommentData()
        {
            return new List<OutOfSpecSpotsToDoDto>()
            {
                new OutOfSpecSpotsToDoDto
                {
                    Id = 3,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    PlanId = 215,
                     SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                    HouseIsci = "289J76GN16H",
                    Comment = "test Comment",
                    GenreName="Comedy",
                    DaypartCode="ROSP",
                    InventorySourceName = "TVB"
                },
                new OutOfSpecSpotsToDoDto
                {
                    Id = 4,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Cincinnati",
                    PlanId = 215,
                     SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0NDAxOA==",
                    HouseIsci = "289J76GN16H",
                    Comment = "test Comment",
                    InventorySourceName = "TVB"
                }
            };
        }

        private List<OutOfSpecSpotsToDoDto> _GetOutOfSpecPlanSpotSingleAddUpdateCommentData()
        {
            return new List<OutOfSpecSpotsToDoDto>()
            {
                new OutOfSpecSpotsToDoDto
                {
                    Id = 3,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    PlanId = 215,
                     SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                    HouseIsci = "289J76GN16H",
                    Comment = "test Comment",
                    GenreName="Comedy",
                    DaypartCode="ROSP",
                    InventorySourceName = "TVB"
                },
                new OutOfSpecSpotsToDoDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY1MzI1MjM=",
                    ExecutionIdExternal = "2212161638441z7VnXU_R3",
                    ReasonCodeMessage = null,
                    EstimateId = 2009,
                    IsciName = "JARD0075000H",
                    RecommendedPlanId = 674,
                    RecommendedPlanName = "Boehringer Jardiance 4Q22 BYU EM News",
                    ProgramName = "BUSINESS FIRST AM",
                    StationLegacyCallLetters = "KOLD",
                    Affiliate= "CBS",
                    Market = "Tucson (Sierra Vista)",
                    AdvertiserMasterId = new Guid("1d0fa038-6a70-4907-b9ba-739ab67e35ad"),
                    AdvertiserName = null,
                    SpotLengthId = null,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 2,
                        Length= 60
                    },
                    AudienceId = null,
                    Audience = new AudienceDto
                    {
                        Id = 40,
                        Code = "A35-64",
                        Name = "Adults 35-64"
                    },
                    Product = null,
                    FlightStartDate = new DateTime(2022, 12, 12),
                    FlightEndDate = new DateTime(2022, 12, 25),
                    DaypartCode = "EMN",
                    GenreName = "INFORMATIONAL/NEWS",
                    DaypartDetail =
                    {
                        Id = 0,
                        Code = null,
                        Name = null,
                        DaypartText = null
                    },
                    ProgramNetwork = "CBS",
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    IngestedBy = "Test User",
                    IngestedAt = new DateTime(2022, 12, 12),
                    Impressions = 464.37199999999996,
                    IngestedMediaWeekId = 989,
                    PlanId = 674,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Description = null,
                        Label = "Time"
                    },
                    MarketCode = 289,
                    MarketRank = 69,
                    HouseIsci = "009ARD0075H",
                    TimeZone = "EST",
                    DMA = 58,
                    Comment = null,
                    InventorySourceName = "Business First AM"
                }
            };
        }

        private List<OutOfSpecSpotsToDoDto> _GetOutOfSpecPlanSpotsData()
        {
            return new List<OutOfSpecSpotsToDoDto>()
            {
                new OutOfSpecSpotsToDoDto
                {
                    Id = 3,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                    PlanId = 215,
                     SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                    HouseIsci = "289J76GN16H",
                    Comment = "test Comment",
                    GenreName="Comedy",
                    DaypartCode="ROSP",
                    InventorySourceName = "TVB"
                },
                new OutOfSpecSpotsToDoDto
                {
                    Id = 4,
                    ReasonCodeMessage = "",
                    EstimateId = 191760,
                    IsciName = "CC44ZZPT4",
                    RecommendedPlanId = 215,
                    RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                    ProgramName = "Reckitt HYHO",
                    StationLegacyCallLetters = "KXMC",
                    AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                    Affiliate = "CBS",
                    Market = "Cincinnati",
                    PlanId = 215,
                     SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                    AudienceId = 426,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2019, 12, 1),
                    FlightEndDate = new DateTime(2019, 12, 9),
                    Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 71646,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                    IngestedAt = new DateTime(2019,1,1),
                    IngestedBy = "Repository Test User",
                    IngestedMediaWeekId = 1,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 2,
                        ReasonCode = 1,
                        Reason = "spot aired outside daypart",
                        Label = "Daypart"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0NDAxOA==",
                    HouseIsci = "289J76GN16H",
                    Comment = "test Comment",
                    InventorySourceName = "TVB"
                },
                new OutOfSpecSpotsToDoDto
                {
                    Id = 14,
                    SpotUniqueHashExternal = "TE9DQUwtNDY1MzI1MjM=",
                    ExecutionIdExternal = "2212161638441z7VnXU_R3",
                    ReasonCodeMessage = null,
                    EstimateId = 2009,
                    IsciName = "JARD0075000H",
                    RecommendedPlanId = 674,
                    RecommendedPlanName = "Boehringer Jardiance 4Q22 BYU EM News",
                    ProgramName = "BUSINESS FIRST AM",
                    StationLegacyCallLetters = "KOLD",
                    Affiliate= "CBS",
                    Market = "Tucson (Sierra Vista)",
                    AdvertiserMasterId = new Guid("1d0fa038-6a70-4907-b9ba-739ab67e35ad"),
                    AdvertiserName = null,
                    SpotLengthId = null,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 2,
                        Length= 60
                    },
                    AudienceId = null,
                    Audience = new AudienceDto
                    {
                        Id = 40,
                        Code = "A35-64",
                        Name = "Adults 35-64"
                    },
                    Product = null,
                    FlightStartDate = new DateTime(2022, 12, 12),
                    FlightEndDate = new DateTime(2022, 12, 25),
                    DaypartCode = "EMN",
                    GenreName = "INFORMATIONAL/NEWS",
                    DaypartDetail =
                    {
                        Id = 0,
                        Code = null,
                        Name = null,
                        DaypartText = null
                    },
                    ProgramNetwork = "CBS",
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    IngestedBy = "Test User",
                    IngestedAt = new DateTime(2022, 12, 12),
                    Impressions = 464.37199999999996,
                    IngestedMediaWeekId = 989,
                    PlanId = 674,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Description = null,
                        Label = "Time"
                    },
                    MarketCode = 289,
                    MarketRank = 69,
                    HouseIsci = "009ARD0075H",
                    TimeZone = "EST",
                    DMA = 58,
                    Comment = null,
                    InventorySourceName = "Business First AM"
                },
                new OutOfSpecSpotsToDoDto
                {
                    Id = 15,
                    SpotUniqueHashExternal = "TE9DQUwtNDY1MzI1MjM=",
                    ExecutionIdExternal = "2212161638441z7VnXU_R3",
                    ReasonCodeMessage = null,
                    EstimateId = 2009,
                    IsciName = "JARD0075000H",
                    RecommendedPlanId = 674,
                    RecommendedPlanName = "Boehringer Jardiance 4Q22 BYU EM News",
                    ProgramName = "BUSINESS FIRST AM",
                    StationLegacyCallLetters = "KOLD",
                    Affiliate= "CBS",
                    Market = "Tucson (Sierra Vista)",
                    AdvertiserMasterId = new Guid("1d0fa038-6a70-4907-b9ba-739ab67e35ad"),
                    AdvertiserName = null,
                    SpotLengthId = null,
                    SpotLength = new SpotLengthDto
                    {
                        Id = 2,
                        Length= 60
                    },
                    AudienceId = null,
                    Audience = new AudienceDto
                    {
                        Id = 40,
                        Code = "A35-64",
                        Name = "Adults 35-64"
                    },
                    Product = null,
                    FlightStartDate = new DateTime(2022, 12, 12),
                    FlightEndDate = new DateTime(2022, 12, 25),
                    DaypartCode = "EMN",
                    GenreName = "INFORMATIONAL/NEWS",
                    DaypartDetail =
                    {
                        Id = 0,
                        Code = null,
                        Name = null,
                        DaypartText = null
                    },
                    ProgramNetwork = "CBS",
                    ProgramAirTime = new DateTime(2022, 12, 23),
                    IngestedBy = "Test User",
                    IngestedAt = new DateTime(2022, 12, 12),
                    Impressions = 464.37199999999996,
                    IngestedMediaWeekId = 989,
                    PlanId = 674,
                    OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Description = null,
                        Label = "Time"
                    },
                    MarketCode = 289,
                    MarketRank = 69,
                    HouseIsci = "009ARD0075H",
                    TimeZone = "EST",
                    DMA = 58,
                    Comment = null,
                    InventorySourceName = "Business First AM"
                }
            };
        }
                
        private List<OutOfSpecSpotsDoneDto> _GetOutOfSpecDoneData()
        {
            return new List<OutOfSpecSpotsDoneDto>()
            {
                new OutOfSpecSpotsDoneDto
                {
                  Id = 3,
                  ReasonCodeMessage="",
                  EstimateId= 191758,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 218,
                  RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="TEN O'CLOCK NEWS",
                  AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                  StationLegacyCallLetters = "KSTP",
                  Affiliate = "ABC",
                  Market = "Lincoln & Hastings-Krny",
                  PlanId = 218,
                   SpotLength = new SpotLengthDto
                    {
                        Id = 16,
                        Length = 45
                    },
                  AudienceId= 426,
                  Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 70642,
                        Code = "CUS"
                    },
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartCode="PT",
                  GenreName="Horror",
                  ProgramNetwork = "",
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  IngestedMediaWeekId = 1,
                  OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    },
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxNTkzNQ==",
                  HouseIsci = "289J76GN16H"
                },
                new OutOfSpecSpotsDoneDto
                {
                  Id = 4,
                  ReasonCodeMessage="",
                  EstimateId= 191759,
                  IsciName="AB44NR59",
                  RecommendedPlanId= 11726,
                  RecommendedPlanName="2Q' 21 Reynolds Foil TDN and SYN Upfront",
                  ProgramName="TEN O'CLOCK NEWS",
                  AdvertiserMasterId = new Guid("3A9C5C03-3CE7-4652-955A-A6EA8CBC82FB"),
                  StationLegacyCallLetters = "KSTP",
                  Affiliate = "ABC",
                  Market = "Lincoln & Hastings-Krny",
                  PlanId = 218,
                  SpotLength = new SpotLengthDto
                  {
                    Id = 16,
                    Length = 45
                  },
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartCode="PT",
                  GenreName="Horror",
                  Audience = new AudienceDto
                    {
                        Id = 426,
                        Code = "M50-64",
                        Name = "Men 50-64"
                    },
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 70642,
                        Code = "CUS"
                    },
                    ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                  IngestedAt = new DateTime(2019,1,1),
                  IngestedBy = "Repository Test User",
                  IngestedMediaWeekId = 1,
                  OutOfSpecSpotReasonCodes = new OutOfSpecSpotReasonCodesDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    },
                    SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxOTY3MA==",
                    HouseIsci = "289J76GN16H"
                }
            };
        }

        //ovo
        private List<OutOfSpecPlansDto> _GetOutOfSpecPlansToDo()
        {
            return new List<OutOfSpecPlansDto>
            {
                new OutOfSpecPlansDto
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
                new OutOfSpecPlansDto
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
