﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Helpers.Json;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class CampaignServiceUnitTests
    {
        private const string _User = "UnitTest_User";
        private const string _TemplatesFilePath = "\\templates";
        private readonly DateTime _CurrentDate = new DateTime(2017, 10, 17, 7, 30, 23);


        private Mock<IBroadcastLockingManagerApplicationService> _LockingManagerApplicationServiceMock;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ICampaignRepository> _CampaignRepositoryMock;
        private Mock<ICampaignValidator> _CampaignValidatorMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock;
        private Mock<ICampaignAggregator> _CampaignAggregatorMock;
        private Mock<IAudienceService> _AudienceServiceMock;
        private Mock<IStandardDaypartService> _StandardDaypartServiceMock;
        private Mock<ISharedFolderService> _SharedFolderServiceMock;
        private Mock<ICampaignAggregationJobTrigger> _CampaignAggregationJobTriggerMock;
        private Mock<ICampaignSummaryRepository> _CampaignSummaryRepositoryMock;
        private Mock<IPlanRepository> _PlanRepositoryMock;
        private Mock<IInventoryRepository> _InventoryRepositoryMock;
        private Mock<IMarketCoverageRepository> _MarketCoverageRepositoryMock;
        private Mock<IStationProgramRepository> _StationProgramRepositoryMock;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IWeeklyBreakdownEngine> _WeeklyBreakdownEngineMock;
        private readonly Mock<IDaypartCache> _DaypartCacheMock = new Mock<IDaypartCache>();
        private Mock<ISpotLengthRepository> _SpotLengthRepositoryMock;
        private Mock<IAabEngine> _AabEngine;
        private Mock<IFeatureToggleHelper> _FeatureToggleHelper;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
        private Mock<ILockingEngine> _LockingEngineMock;
        private Mock<IPlanService> _PlanServiceMock;
        private Mock<IPlanValidator> _PlanValidatorMock;
        private Mock<ICampaignServiceApiClient> _CampaignServiceApiClientMock;

        [SetUp]
        public void SetUp()
        {
            _LockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _CampaignRepositoryMock = new Mock<ICampaignRepository>();
            _CampaignValidatorMock = new Mock<ICampaignValidator>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            _CampaignAggregatorMock = new Mock<ICampaignAggregator>();
            _AudienceServiceMock = new Mock<IAudienceService>();
            _StandardDaypartServiceMock = new Mock<IStandardDaypartService>();
            _SharedFolderServiceMock = new Mock<ISharedFolderService>();
            _CampaignAggregationJobTriggerMock = new Mock<ICampaignAggregationJobTrigger>();
            _CampaignSummaryRepositoryMock = new Mock<ICampaignSummaryRepository>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();
            _MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();
            _InventoryRepositoryMock = new Mock<IInventoryRepository>();
            _StationProgramRepositoryMock = new Mock<IStationProgramRepository>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _WeeklyBreakdownEngineMock = new Mock<IWeeklyBreakdownEngine>();
            _SpotLengthRepositoryMock = new Mock<ISpotLengthRepository>();
            _AabEngine = new Mock<IAabEngine>();
            _FeatureToggleHelper = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();
            _LockingEngineMock = new Mock<ILockingEngine>();
            _PlanServiceMock = new Mock<IPlanService>();
            _PlanValidatorMock = new Mock<IPlanValidator>();
            _CampaignServiceApiClientMock = new Mock<ICampaignServiceApiClient>();
            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IStationProgramRepository>())
                .Returns(_StationProgramRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanRepository>())
                .Returns(_PlanRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ICampaignRepository>())
                .Returns(_CampaignRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ICampaignSummaryRepository>())
                .Returns(_CampaignSummaryRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IMarketCoverageRepository>())
                .Returns(_MarketCoverageRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotLengthRepository>())
                .Returns(_SpotLengthRepositoryMock.Object);
        }

        [Test]
        public void ReturnsFilteredCampaigns()
        {
            // Arrange
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Name2", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithsummeriesReturn(agency, advertiser));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29)
                , new DateTime(2009, 3, 29)));

            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetCampaigns(new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Year = 2019,
                    Quarter = 2
                },
                PlanStatus = PlanStatusEnum.ClientApproval
            }, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ReturnsCampaignsFilteredUsingDefaultFilter()
        {
            // Arrange
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Name2", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
                .Returns(_GetCampaignWithsummeriesReturn(agency, advertiser));

            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetCampaigns(null, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ThrowsException_WhenCanNotGetAllCampaignsFromRepository()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from GetAllCampaigns";

            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(It.IsAny<int?>(), It.IsAny<int?>())).Returns(new DateRange(null, null));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterRangeByDate(_CurrentDate)).Returns(new QuarterDetailDto());
            _CampaignRepositoryMock
                .Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null))
                .Callback(() => throw new Exception(expectedMessage));

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetCampaigns(It.IsAny<CampaignFilterDto>(), _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ReturnFilteredCampaignsWithDraftExcluded()
        {
            // Arrange
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Name2", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithPlanswithdraftExcludedReturn(agency, advertiser));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29)
                , new DateTime(2009, 3, 29)));

            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetCampaigns(new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Year = 2019,
                    Quarter = 2
                },
                PlanStatus = (PlanStatusEnum)1
            }, _CurrentDate); ;

            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private static List<CampaignWithSummary> _GetCampaignWithPlanswithdraftExcludedReturn(AgencyDto agency, AdvertiserDto advertiser)
        {
            return new List<CampaignWithSummary>
            {
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 1,
                        Name = "CampaignOne",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignOne.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                        HasPlans = true,
                        Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2354,
                            Status = (PlanStatusEnum)5,
                            DraftId = 2354,
                            IsDraft = false,
                            VersionNumber = 4
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2355,
                            Status = (PlanStatusEnum)5,
                            DraftId = null,
                            IsDraft = true,
                            VersionNumber = 4
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 575,
                            Name = "plan-2002",
                            VersionId = 2278,
                            Status = (PlanStatusEnum)1,
                            DraftId = 2278,
                            IsDraft = false,
                            VersionNumber = 1
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 575,
                            Name = "plan-2002",
                            VersionId = 2347,
                            Status = (PlanStatusEnum)5,
                            DraftId = 2347,
                            IsDraft = false,
                            VersionNumber = 2
                        }
                    }

                    },
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 2,
                        Name = "CampaignTwo",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignTwo.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                        HasPlans = true,
                         Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2354,
                            Status = (PlanStatusEnum)5,
                            DraftId = 2354,
                            IsDraft = false,
                            VersionNumber = 4
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2355,
                            Status = (PlanStatusEnum)5,
                            DraftId = null,
                            IsDraft = true,
                            VersionNumber = 4
                        }

                    }
                    }
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 3,
                        Name = "CampaignThree",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignThree.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                        HasPlans = false
                    }
                }
            };
        }

        [Test]
        public void ThrowException_WhenReturnFilteredCampaignsWithDraftExcluded()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from ReturnFilteredCampaignsWithDraftExcluded.";
            // Arrange
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Name2", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithPlanswithdraftExcludedReturn(agency, advertiser))
                .Callback(() => throw new Exception(expectedMessage));

            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29)
                , new DateTime(2009, 3, 29)))
                .Callback(() => throw new Exception(expectedMessage));

            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency)
                .Callback(() => throw new Exception(expectedMessage));

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser)
                .Callback(() => throw new Exception(expectedMessage));

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetCampaigns(new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Year = 2019,
                    Quarter = 2
                },
                PlanStatus = (PlanStatusEnum)1
            }, _CurrentDate));


            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>()), Times.Once);

            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ValidatesCampaign_WhenCreatingCampaign()
        {
            // Arrange
            const int campaignId = 0;
            const string campaignName = "CampaignOne";
            const string campaignNotes = "Notes for CampaignOne.";
            const int advertiserId = 1;
            const int agencyId = 1;
            var campaign = new SaveCampaignDto
            {
                Id = campaignId,
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId,
                Notes = campaignNotes
            };

            var tc = _BuildCampaignService();

            // Act
            tc.SaveCampaign(campaign, _User, _CurrentDate);

            // Assert
            _CampaignValidatorMock.Verify(x => x.Validate(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId)), Times.Once);
        }

        [Test]
        public void CreatesCampaign()
        {
            // Arrange
            const int campaignId = 0;
            const string campaignName = "CampaignOne";
            const string campaignNotes = "Notes for CampaignOne.";
            const int advertiserId = 1;
            const int agencyId = 1;
            var campaign = new SaveCampaignDto
            {
                Id = campaignId,
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId,
                Notes = campaignNotes
            };

            var tc = _BuildCampaignService();

            // Act
            tc.SaveCampaign(campaign, _User, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId),
                _User,
                _CurrentDate), Times.Once);
        }

        [Test]
        public void ThrowsException_WhenCreatingInvalidCampaign()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from Validate.";
            const int campaignId = 0;
            const string campaignName = "CampaignOne";
            const string campaignNotes = "Notes for CampaignOne.";
            const int advertiserId = 1;
            const int agencyId = 1;
            var campaign = new SaveCampaignDto
            {
                Id = campaignId,
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId,
                Notes = campaignNotes
            };

            _CampaignValidatorMock
                .Setup(s => s.Validate(It.IsAny<SaveCampaignDto>()))
                .Callback<SaveCampaignDto>(x => throw new Exception(expectedMessage));

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _User, _CurrentDate));

            // Assert
            _CampaignValidatorMock.Verify(x => x.Validate(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId)), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.CreateCampaign(It.IsAny<SaveCampaignDto>(), _User, _CurrentDate), Times.Never);
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_WhenCanNotCreateCampaign()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from CreateCampaign.";
            const int campaignId = 0;
            const string campaignName = "CampaignOne";
            const string campaignNotes = "Notes for CampaignOne.";
            const int advertiserId = 1;
            const int agencyId = 1;
            var campaign = new SaveCampaignDto
            {
                Id = campaignId,
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId,
                Notes = campaignNotes
            };

            _CampaignRepositoryMock
                .Setup(s => s.CreateCampaign(It.IsAny<SaveCampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => throw new Exception(expectedMessage));

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _User, _CurrentDate));

            // Assert
            _CampaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId),
                _User,
                _CurrentDate), Times.Once);
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetQuarters()
        {
            // Arrange
            var getCampaignsDateRangesReturn = new List<DateRange>
            {
                new DateRange(null, null),
                new DateRange(new DateTime(2019, 1, 1), null),
                new DateRange(new DateTime(2019, 2, 1), new DateTime(2019, 9, 1))
            };
            _CampaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(null)).Returns(getCampaignsDateRangesReturn);

            _QuarterCalculationEngineMock
                .Setup(x => x.GetQuartersForDateRanges(It.IsAny<List<DateRange>>()))
                .Returns(new List<QuarterDetailDto>
                {
                    new QuarterDetailDto
                    {
                        Quarter = 1,
                        Year = 2019,
                        StartDate = new DateTime(2018, 12, 31),
                        EndDate = new DateTime(2019, 03, 31, 23, 59, 59)
                    },
                    new QuarterDetailDto
                    {
                        Quarter = 2,
                        Year = 2019,
                        StartDate = new DateTime(2019, 04, 01),
                        EndDate = new DateTime(2019, 06, 30, 23, 59, 59)
                    },
                    new QuarterDetailDto
                    {
                        Quarter = 3,
                        Year = 2019,
                        StartDate = new DateTime(2018, 07, 01),
                        EndDate = new DateTime(2019, 09, 29, 23, 59, 59)
                    }
                });

            _QuarterCalculationEngineMock
                .Setup(x => x.GetQuarterRangeByDate(It.IsAny<DateTime>()))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 3,
                    Year = 2019,
                    StartDate = new DateTime(2018, 07, 01),
                    EndDate = new DateTime(2019, 09, 29, 23, 59, 59)
                });

            var tc = _BuildCampaignService();

            // Act
            var campaignQuarters = tc.GetQuarters(null, new DateTime(2019, 8, 20));

            // Assert
            Assert.IsNotNull(campaignQuarters);
            Assert.AreEqual(3, campaignQuarters.Quarters.Count);
            Assert.AreEqual(3, campaignQuarters.DefaultQuarter.Quarter);
            Assert.AreEqual(2019, campaignQuarters.DefaultQuarter.Year);
        }

        [Test]
        public void TriggerCampaignAggregation()
        {
            const int campaignId = 1;

            var tc = _BuildCampaignService();

            tc.TriggerCampaignAggregationJob(campaignId, _User);

            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(campaignId, _User), Times.Once);
        }

        [Test]
        public void ProcessAggregation()
        {
            const int campaignId = 1;

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(It.IsAny<PlanStatusEnum?>())).Returns(new List<DateRange>());
            _CampaignAggregatorMock.Setup(s => s.Aggregate(It.IsAny<int>()))
                .Returns(new CampaignSummaryDto());
            _CampaignSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()))
                .Returns(1);

            var tc = _BuildCampaignService();

            tc.ProcessCampaignAggregation(campaignId);

            _CampaignAggregatorMock.Verify(s => s.Aggregate(campaignId), Times.Once);
            _CampaignSummaryRepositoryMock.Verify(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()), Times.Once);
            _CampaignSummaryRepositoryMock.Verify(s => s.SetSummaryProcessingStatusToError(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ProcessAggregation_WithError()
        {
            const int campaignId = 1;

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(It.IsAny<PlanStatusEnum?>())).Returns(new List<DateRange>());
            _CampaignAggregatorMock.Setup(s => s.Aggregate(It.IsAny<int>()))
                .Returns(new CampaignSummaryDto());
            _CampaignSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()))
                .Throws(new Exception("Error from SaveSummary"));

            var tc = _BuildCampaignService();

            var caught = Assert.Throws<Exception>(() => tc.ProcessCampaignAggregation(campaignId));

            Assert.AreEqual("Error from SaveSummary", caught.Message);
            _CampaignAggregatorMock.Verify(s => s.Aggregate(campaignId), Times.Once);
            _CampaignSummaryRepositoryMock.Verify(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()), Times.Once);
            _CampaignSummaryRepositoryMock.Verify(s =>
                s.SetSummaryProcessingStatusToError(campaignId, $"Exception caught during processing : Error from SaveSummary"), Times.Once);
        }

        [Test]
        public void CanNotGenerateCampaignExport_WhenCampaignIsLocked()
        {
            // Arrange
            const int campaignId = 5;
            const string lockedUserName = "UnitTestsUser2";
            var expectedMessage = $"Campaign with id {campaignId} has been locked by {lockedUserName}";

            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Contract
            };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(new CampaignDto
                {
                    Id = campaignId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetCampaignLockingKey(campaignId)))
                .Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = lockedUserName
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetAndValidateCampaignReportData(request));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateProgramLineup_WhenCampaignIsLocked()
        {
            // Arrange
            const int campaignId = 6;
            const int planId = 1197;
            const string lockedUserName = "UnitTestsUser2";
            var expectedMessage = $"Campaign with id {campaignId} has been locked by {lockedUserName}";

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { planId }
            };

            _CampaignRepositoryMock
               .Setup(x => x.GetCampaign(campaignId))
               .Returns(new CampaignDto
               {
                   Id = campaignId,
                   Plans = new List<PlanSummaryDto>
                   {
                        new PlanSummaryDto
                        {
                            PlanId = planId,
                            PostingType = PostingTypeEnum.NSI,
                            Status = PlanStatusEnum.Complete
                        }
                   }
               });

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), null))
                .Returns(_GetPlan(planId, campaignId));

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetPlanLockingKey(planId)))
                .Returns(new LockResponse
                {
                    Success = true,
                    LockedUserName = lockedUserName
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetCampaignLockingKey(campaignId)))
                .Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = lockedUserName
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetProgramLineupReportData(request, DateTime.Now));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateProgramLineup_WhenPlanIsLocked()
        {
            // Arrange
            const int campaignId = 6;
            const int planId = 1197;
            const string lockedUserName = "UnitTestsUser2";
            var expectedMessage = $"Plan with id {planId} has been locked by {lockedUserName}";

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { planId }
            };

            _CampaignRepositoryMock
               .Setup(x => x.GetCampaign(campaignId))
               .Returns(new CampaignDto
               {
                   Id = campaignId,
                   Plans = new List<PlanSummaryDto>
                   {
                        new PlanSummaryDto
                        {
                            PlanId = planId,
                            PostingType = PostingTypeEnum.NSI,
                            Status = PlanStatusEnum.Complete
                        }
                   }
               });

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), null))
                .Returns(_GetPlan(planId, campaignId));

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetPlanLockingKey(planId)))
                .Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = lockedUserName
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetProgramLineupReportData(request, DateTime.Now));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateCampaignExport_WhenPlanIsLocked()
        {
            // Arrange
            const int campaignId = 5;
            const int planId = 7;
            const string lockedUserName = "UnitTestsUser2";
            var expectedMessage = $"Plan with id {planId} has been locked by {lockedUserName}";

            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                SelectedPlans = new List<int> { planId },
                ExportType = CampaignExportTypeEnum.Contract
            };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = planId,
                            PostingType = PostingTypeEnum.NSI,
                            Status = PlanStatusEnum.Complete
                        }
                    }
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetCampaignLockingKey(campaignId)))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetPlanLockingKey(planId)))
                .Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = lockedUserName
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetAndValidateCampaignReportData(request));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateCampaignExport_WhenPlanAggregation_IsInProgress()
        {
            // Arrange
            const int campaignId = 5;
            const int planId = 7;
            var expectedMessage = $"Data aggregation for the plan with id {planId} is in progress. Please wait until the process is done";

            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                SelectedPlans = new List<int> { planId },
                ExportType = CampaignExportTypeEnum.Contract
            };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = planId,
                            PostingType = PostingTypeEnum.NSI,
                            Status = PlanStatusEnum.Complete,
                            ProcessingStatus = PlanAggregationProcessingStatusEnum.InProgress
                        }
                    }
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetAndValidateCampaignReportData(request));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateCampaignExport_WhenPlanAggregation_HasFailed()
        {
            // Arrange
            const int campaignId = 5;
            const int planId = 7;
            var expectedMessage = $"Data aggregation for the plan with id {planId} has failed. Please contact the support";

            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                SelectedPlans = new List<int> { planId },
                ExportType = CampaignExportTypeEnum.Contract
            };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = planId,
                            PostingType = PostingTypeEnum.NSI,
                            Status = PlanStatusEnum.Complete,
                            ProcessingStatus = PlanAggregationProcessingStatusEnum.Error
                        }
                    }
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetAndValidateCampaignReportData(request));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_ProgramLineup_WhenNoPlansSelected()
        {
            // Arrange
            var expectedMessage = $"Choose at least one plan";
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int>()
            };

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_ProgramLineup_WhenNoPricingRunsDone()
        {
            // Arrange
            const string expectedMessage = "There are no completed pricing runs for the chosen plan. Please run pricing";
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { 1 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Dayparts = _GetPlanDayparts()
                });

            _LockingManagerApplicationServiceMock
                 .Setup(x => x.GetLockObject(It.IsAny<string>()))
                 .Returns(new LockResponse
                 {
                     Success = true
                 });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed, "The latest pricing run was failed. Please run pricing again or contact the support")]
        [TestCase(BackgroundJobProcessingStatus.Queued, "There is a pricing run in progress right now. Please wait until it is completed")]
        [TestCase(BackgroundJobProcessingStatus.Processing, "There is a pricing run in progress right now. Please wait until it is completed")]
        public void ThrowsException_ProgramLineup_PricingJobIsNotAcceptable(
            BackgroundJobProcessingStatus jobStatus,
            string expectedMessage)
        {
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { 1 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    Dayparts = _GetPlanDayparts()
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = jobStatus
                });

            _LockingManagerApplicationServiceMock
                 .Setup(x => x.GetLockObject(It.IsAny<string>()))
                 .Returns(new LockResponse
                 {
                     Success = true
                 });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ReturnsData_ProgramLineupReport_45UnEquivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b");
            Guid advertiserId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 8, Weight = 50 } },
                    Equivalized = false,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    PricingParameters = new PlanPricingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPricingJobForLatestPlanVersion(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ProgramLineup_MultipleCreativeLengths()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b");
            Guid advertiserId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId },

            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> {
                        new CreativeLength { SpotLengthId = 1, Weight = 50 },
                        new CreativeLength { SpotLengthId = 2, Weight = 20 },
                        new CreativeLength { SpotLengthId = 3, Weight = 30 }
                    },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    PricingParameters = new PlanPricingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });
            _PlanRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPricingJobForLatestPlanVersion(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ProgramLineup_MultipleCreativeLengths_UnEquivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b");
            Guid advertiserId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> {
                        new CreativeLength { SpotLengthId = 1, Weight = 50 },
                        new CreativeLength { SpotLengthId = 2, Weight = 20 },
                        new CreativeLength { SpotLengthId = 3, Weight = 30 }
                    },
                    Equivalized = false,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    PricingParameters = new PlanPricingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ReturnsData_ProgramLineup_45Equivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b");
            Guid advertiserId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 8, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    PricingParameters = new PlanPricingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPricingJobForLatestPlanVersion(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ReturnsData_ProgramLineup_30Equivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b");
            Guid advertiserId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    PricingParameters = new PlanPricingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                 .Setup(x => x.GetLockObject(It.IsAny<string>()))
                 .Returns(new LockResponse
                 {
                     Success = true
                 });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPricingJobForLatestPlanVersion(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ProgramLineup_RollupStationSpecificNewsPrograms()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b");
            Guid advertiserId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 30001,
                            StartTimeSeconds = 57600,
                            EndTimeSeconds = 68399,
                            DaypartTypeId = DaypartTypeEnum.News
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 10001,
                            StartTimeSeconds = 14400,
                            EndTimeSeconds = 35999,
                            DaypartTypeId = DaypartTypeEnum.ROS
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 20001,
                            StartTimeSeconds = 39600,
                            EndTimeSeconds = 46799,
                            DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 40001,
                            StartTimeSeconds = 72000,
                            EndTimeSeconds = 299,
                            DaypartTypeId = DaypartTypeEnum.News
                        }
                    },
                    PricingParameters = new PlanPricingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupForRollupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPricingJobForLatestPlanVersion(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 4001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

       
        [Test]
        public void GetAndValidateCampaignReportDataWithCustomDaypart()
        {
            // Arrange
            var planOne = _GetBasePlanForCampaignExport();
            planOne.Name = "Plan One";

            //change daypart start/end time
            var planTwo = _GetBasePlanForCampaignExport();
            planTwo.Id++;
            planTwo.Name = "Plan Two";
            planTwo.Dayparts.First().StartTimeSeconds = 42000;
            planTwo.Dayparts.First().EndTimeSeconds = 44999;

            var plansDict = new Dictionary<int, PlanDto>
            {
                { planOne.Id, planOne },
                { planTwo.Id, planTwo },
            };

            var campaignId = planOne.CampaignId;
            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Proposal,
                SelectedPlans = new List<int> { planOne.Id, planTwo.Id }
            };
            var campaign = _GetCampaignForExport(campaignId, new List<PlanDto> { planOne, planTwo });
            var campaignLocksWell = true;
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Agent1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Advertiser1", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(campaign);
            _PlanRepositoryMock.Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns<int, int?>((i, b) => plansDict[i]);
            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse { Success = campaignLocksWell });
            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);
            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);
            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 01, 01));

            var tc = _BuildCampaignService();
            // Act
            var response = tc.GetAndValidateCampaignReportData(request);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        public void GetAndValidateCampaignReportData_WithDraft()
        {
            // Arrange
            var planOne = _GetBasePlanForCampaignExport();
            planOne.Name = "Plan One";
            planOne.VersionId = 7235;

            //change daypart start/end time
            var planTwo = _GetBasePlanForCampaignExport();
            planTwo.Id++;
            planTwo.Name = "Plan Two";
            planTwo.Dayparts.First().StartTimeSeconds = 42000;
            planTwo.Dayparts.First().EndTimeSeconds = 44999;
            planTwo.VersionId = 7236;
            planTwo.IsDraft = true;

            var plansDict = new Dictionary<int, PlanDto>
            {
                { planOne.Id, planOne },
                { planTwo.Id, planTwo }
            };
            var plansVersionList = new List<int>
            {
                { planOne.VersionId },
                { planTwo.VersionId }
            };

            var campaignId = planOne.CampaignId;
            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Proposal,
                SelectedPlans = new List<int> { planOne.Id }
            };
            var campaign = _GetCampaignForExport(campaignId, new List<PlanDto> { planOne, planTwo });
            campaign.HasPlans = true;
            var campaignLocksWell = true;
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Agent1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Advertiser1", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(campaign);
            _PlanRepositoryMock.Setup(x => x.GetLatestVersionIdForPlan(It.IsAny<int>()))
                 .Returns<int>((i) => plansVersionList[i]);
            _PlanRepositoryMock.Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns<int, int?>((i, b) => plansDict[i]);
            _LockingManagerApplicationServiceMock
                   .Setup(x => x.GetLockObject(It.IsAny<string>()))
                   .Returns(new LockResponse { Success = campaignLocksWell });
            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);
            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);
            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 01, 01));
            var tc = _BuildCampaignService();
            // Act
            var response = tc.GetAndValidateCampaignReportData(request);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        public void GetAndValidateCampaignReportDataWithDuplicateProgramName()
        {
            // Arrange
            var plan = _GetBasePlanForCampaignExport();
            plan.Dayparts.First().Restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
            {
                ContainType = ContainTypeEnum.Exclude,
                Programs = new List<Entities.DTO.Program.ProgramDto>
                {
                    new Entities.DTO.Program.ProgramDto
                    {
                        ContentRating = "PG",
                        Name = "Mom",
                        Genre = new LookupDto{ Id = 9 }
                    },
                    new Entities.DTO.Program.ProgramDto
                    {
                        ContentRating = "PG",
                        Name = "Mom",
                        Genre = new LookupDto{ Id = 53 }
                    }
                }
            };

            var campaignId = plan.CampaignId;
            var planId = plan.Id;
            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Proposal,
                SelectedPlans = new List<int> { planId }
            };
            var campaign = _GetCampaignForExport(campaignId, new List<PlanDto> { plan });
            var campaignLocksWell = true;
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Agent1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Advertiser1", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(campaign);
            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);
            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse { Success = campaignLocksWell });
            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);
            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();
            // Act
            var response = tc.GetAndValidateCampaignReportData(request);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        public void GetAndValidateCampaignReportData()
        {
            // Arrange
            var plan = _GetBasePlanForCampaignExport();
            var campaignId = plan.CampaignId;
            var planId = plan.Id;
            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Proposal,
                SelectedPlans = new List<int> { planId }
            };
            var campaign = _GetCampaignForExport(campaignId, new List<PlanDto> { plan });
            var campaignLocksWell = true;
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Agent1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Advertiser1", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(campaign);
            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);
            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse { Success = campaignLocksWell });
            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);
            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();
            // Act
            var response = tc.GetAndValidateCampaignReportData(request);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        public void GetAndValidateCampaignReportDataWithFluidityAllow()
        {
            // Arrange
            var plan = _GetBasePlanForCampaignExport();
            plan.FluidityPercentage = 40;
            var campaignId = plan.CampaignId;
            var planId = plan.Id;
            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Proposal,
                SelectedPlans = new List<int> { planId }
            };
            var campaign = _GetCampaignForExport(campaignId, new List<PlanDto> { plan });
            var campaignLocksWell = true;
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Agent1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Advertiser1", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(campaign);
            _PlanRepositoryMock.Setup(s => s.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(plan);
            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse { Success = campaignLocksWell });
            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);
            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();
            // Act
            var response = tc.GetAndValidateCampaignReportData(request);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }


        private PlanDto _GetBasePlanForCampaignExport()
        {
            const string fileName = "BasePlanForCampaignExportUnitTests.json";
            const string directoryName = @".\Files\Campaign export";
            var filePath = Path.Combine(directoryName, fileName);
            var planJson = File.ReadAllText(filePath);
            var plan = JsonSerializerHelper.ConvertFromJson<PlanDto>(planJson);
            return plan;
        }

        private static MediaWeek _GetMediaWeek(int id, string weekStartDate = null, string weekEndDate = null, int? mediaMonthId = null)
        {
            return new MediaWeek
            {
                Id = id,
                StartDate = weekStartDate == null ? new DateTime() : Convert.ToDateTime(weekStartDate),
                EndDate = weekEndDate == null ? new DateTime() : Convert.ToDateTime(weekEndDate),
                MediaMonthId = mediaMonthId ?? 0
            };
        }

        private List<MediaWeek> _GetTheSameMediaWeeksAsThePlan(PlanDto planDto)
        {
            var result = new List<MediaWeek>();
            planDto.WeeklyBreakdownWeeks.Select(x => x.MediaWeekId)
                .ToList()
                .ForEach(mediaWeekId => result.Add(
                    _GetMediaWeek(mediaWeekId)));
            return result;
        }

        private List<StandardDaypartDto> _GetStandardDayparts()
        {
            return new List<StandardDaypartDto>
            {
                new StandardDaypartDto
                {
                    Code = "MDN",
                    Id = 2
                },
                new StandardDaypartDto
                {
                    Code = "EF",
                    Id = 6
                },
                new StandardDaypartDto
                {
                    Code = "EM",
                    Id = 14
                },
            };
        }

        private void _SetupBaseProgramLineupForRollupTestData()
        {
            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime(2020, 02, 12)
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(_GetPlanPricingAllocatedSpotsForRollup());

            _InventoryRepositoryMock
                .Setup(x => x.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetStationInventoryManifestsForRollup());

            _AabEngine
                .Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(new AgencyDto
                {
                    Name = "1st Image Marketing"
                });

            _AabEngine
                .Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(new AdvertiserDto
                {
                    Name = "PetMed Express, Inc"
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoveragesWithStations())
                .Returns(_GetMarketCoverages());

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, BasePlanInventoryProgram.ManifestDaypart.Program>
                {
                    {
                        1001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 1001 Morning NEWS"
                        }
                    },
                    {
                        1002,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 1002 Morning NEWS"
                        }
                    },
                    {
                        2001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 2001 Midday NEWS"
                        }
                    },
                     {
                        2002,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 2002 Midday NEWS"
                        }
                    },
                     {
                        3001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 3001 Evening NEWS"
                        }
                    },
                     {
                        3002,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 3002 Evening NEWS"
                        }
                    },
                     {
                        4001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 4001 Late NEWS"
                        }
                    },
                     {
                        4002,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "KPLR 4002 Late NEWS"
                        }
                    },
                });
        }

        private void _SetupBaseProgramLineupTestData()
        {
            _PlanRepositoryMock
                .Setup(x => x.GetPricingJobForLatestPlanVersion(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime(2020, 02, 12)
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanId(It.IsAny<int>(), It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()))
                .Returns(_GetPlanPricingAllocatedSpots());

            _InventoryRepositoryMock
                .Setup(x => x.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetStationInventoryManifests());

            _AabEngine
                .Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(new AgencyDto
                {
                    Name = "1st Image Marketing"
                });

            _AabEngine
                .Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(new AdvertiserDto
                {
                    Name = "PetMed Express, Inc"
                });

            _AudienceServiceMock
                .Setup(x => x.GetAudienceById(It.IsAny<int>()))
                .Returns(new PlanAudienceDisplay
                {
                    Code = "A18-20"
                });

            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());

            _SpotLengthRepositoryMock.Setup(s => s.GetDeliveryMultipliersBySpotLengthId())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoveragesWithStations())
                .Returns(_GetMarketCoverages());

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, BasePlanInventoryProgram.ManifestDaypart.Program>
                {
                    {
                        1001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "Movie",
                            Name = "Joker"
                        }
                    },
                    {
                        2001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "Late News"
                        }
                    },
                    {
                        3001,
                        new BasePlanInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "Movie",
                            Name = "1917"
                        }
                    }
                });
        }

        private MarketCoverageByStation _GetMarketCoverages()
        {
            return new MarketCoverageByStation
            {
                Markets = new List<MarketCoverageByStation.Market>
                {
                    new MarketCoverageByStation.Market
                    {
                        MarketCode = 100,
                        Rank = 2,
                        MarketName = "Binghamton",
                        Stations = new List<MarketCoverageByStation.Station>
                        {
                            new MarketCoverageByStation.Station
                            {
                                Id = 101,
                                LegacyCallLetters = "A101"
                            }
                        }
                    },
                    new MarketCoverageByStation.Market
                    {
                        MarketCode = 200,
                        Rank = 1,
                        MarketName = "New York",
                        Stations = new List<MarketCoverageByStation.Station>
                        {
                            new MarketCoverageByStation.Station
                            {
                                Id = 201,
                                LegacyCallLetters = "B201"
                            }
                        }
                    },
                    new MarketCoverageByStation.Market
                    {
                        MarketCode = 300,
                        Rank = 3,
                        MarketName = "Macon",
                        Stations = new List<MarketCoverageByStation.Station>
                        {
                            new MarketCoverageByStation.Station
                            {
                                Id = 301,
                                LegacyCallLetters = "C301"
                            }
                        }
                    }
                }
            };
        }

        private List<PlanPricingAllocatedSpot> _GetPlanPricingAllocatedSpotsForRollup()
        {
            return new List<PlanPricingAllocatedSpot>
            {
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 10,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 10001,
                        Code = "EMN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 10,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 10002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 20001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 20
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 20002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 20
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 30001,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 30002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 40001,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 40002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                }
            };
        }

        private List<PlanPricingAllocatedSpot> _GetPlanPricingAllocatedSpots()
        {
            return new List<PlanPricingAllocatedSpot>
            {
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 10,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 10001,
                        Code = "EMN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 20001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 20,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 20
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 30002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 2,
                            Impressions = 10,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 2,
                            Impressions = 10,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 2,
                            Impressions = 10,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 40001,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 1,
                            Impressions = 10,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 50,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 50001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 1
                        },
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 2
                        },
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 3
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 60,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 60001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 70,
                    StandardDaypart = new StandardDaypartDto
                    {
                        Id = 60001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 5,
                            Impressions = 10,
                            SpotLengthId = 1
                        }
                    },
                    Impressions30sec = 10
                }
            };
        }

        private List<StationInventoryManifest> _GetStationInventoryManifestsForRollup()
        {
            return new List<StationInventoryManifest>
                {
                    new StationInventoryManifest
                    {
                        Id = 10,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 100,
                            LegacyCallLetters = "KSTP",
                            Affiliation = "ABC"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 1001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 10001,
                                    StartTime = 14400,
                                    EndTime = 35999,
                                    Monday = true,
                                    Tuesday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 20,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 200,
                            LegacyCallLetters = "WTTV",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 2001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 20001,
                                    StartTime = 39600,
                                    EndTime = 46799,
                                    Wednesday = true,
                                    Thursday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 30,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 300,
                            LegacyCallLetters = "KELO",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 3001,
                                ProgramName = "The Shawshank Redemption",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 30001,
                                    StartTime = 57600,
                                    EndTime = 68399,
                                    Friday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 40,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 300,
                            LegacyCallLetters = "KELOW",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 4001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 40001,
                                    StartTime = 72000,
                                    EndTime = 299,
                                    Friday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                };
        }

        private List<StationInventoryManifest> _GetStationInventoryManifests()
        {
            return new List<StationInventoryManifest>
                {
                    new StationInventoryManifest
                    {
                        Id = 10,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 100,
                            LegacyCallLetters = "KSTP",
                            Affiliation = "ABC"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 1001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 10001,
                                    StartTime = 100,
                                    EndTime = 199,
                                    Monday = true,
                                    Tuesday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 20,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 200,
                            LegacyCallLetters = "WTTV",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 2001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 20001,
                                    StartTime = 200,
                                    EndTime = 299,
                                    Wednesday = true,
                                    Thursday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 30,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 300,
                            LegacyCallLetters = "KELO",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 3001,
                                ProgramName = "The Shawshank Redemption",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 30001,
                                    StartTime = 300,
                                    EndTime = 399,
                                    Friday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 40,
                        Station = null
                    },
                    new StationInventoryManifest
                    {
                        Id = 50,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = null
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 60,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 200,
                            LegacyCallLetters = "WTTV",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 6001,
                                ProgramName = "Fallback program",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 60001,
                                    StartTime = 200,
                                    EndTime = 299,
                                    Saturday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 70,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 200,
                            LegacyCallLetters = "WTTV",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 6001,
                                ProgramName = "Fallback program",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 60001,
                                    StartTime = 200,
                                    EndTime = 299,
                                    Saturday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                };
        }

        private CampaignService _BuildCampaignService(bool isAabEnabled = false, bool isLockingConsolidationEnabled = false)
        {
            // Tie in base data.
            _AudienceServiceMock.Setup(s => s.GetAudienceById(It.IsAny<int>()))
                .Returns<int>(AudienceTestData.GetAudienceById);
            _AudienceServiceMock.Setup(s => s.GetAudiences())
                .Returns(AudienceTestData.GetAudiences());
            _SpotLengthRepositoryMock
                .Setup(x => x.GetSpotLengths())
                .Returns(SpotLengthTestData.GetAllSpotLengths());
            _SpotLengthRepositoryMock.Setup(s => s.GetDeliveryMultipliersBySpotLengthId())
                .Returns(SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId);
            _StandardDaypartServiceMock.Setup(s => s.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData);
            _QuarterCalculationEngineMock
                .Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns<DateTime, DateTime>(QuartersTestData.GetAllQuartersBetweenDates);
            _QuarterCalculationEngineMock.Setup(s => s.GetQuarterRangeByDate(It.IsAny<DateTime>()))
                .Returns<DateTime>(QuartersTestData.GetQuarterRangeByDate);
            _QuarterCalculationEngineMock.Setup(s => s.GetQuarterDetail(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>(QuartersTestData.GetQuarterDetail);
            _QuarterCalculationEngineMock.Setup(s => s.GetQuarterDateRange(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>(QuartersTestData.GetQuarterDateRange);
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns<DateTime, DateTime>(MediaMonthAndWeekTestData.GetMediaWeeksIntersecting);
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaWeeksInRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns<DateTime, DateTime>(MediaMonthAndWeekTestData.GetMediaWeeksIntersecting);
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaMonthsBetweenDatesInclusive(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns<DateTime, DateTime>(MediaMonthAndWeekTestData.GetMediaMonthsIntersecting);
            _MediaMonthAndWeekAggregateCacheMock.Setup(s => s.GetMediaWeeksByMediaMonth(It.IsAny<int>()))
                .Returns<int>(MediaMonthAndWeekTestData.GetMediaWeeksByMediaMonth);
            _LaunchDarklyClientStub = new LaunchDarklyClientStub();                  
            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);
            return new CampaignService(
                _DataRepositoryFactoryMock.Object,
                _CampaignValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _QuarterCalculationEngineMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                _CampaignAggregatorMock.Object,
                _CampaignAggregationJobTriggerMock.Object,
                _AudienceServiceMock.Object,
                _StandardDaypartServiceMock.Object,
                _SharedFolderServiceMock.Object,
                _DateTimeEngineMock.Object,
                _WeeklyBreakdownEngineMock.Object,
                _DaypartCacheMock.Object,
                featureToggleHelper,
                _AabEngine.Object,
                _ConfigurationSettingsHelperMock.Object,
                _LockingEngineMock.Object,
                _PlanServiceMock.Object,
                _PlanValidatorMock.Object,
                _CampaignServiceApiClientMock.Object
               
                );
        }

        private CampaignDto _GetCampaignForExport(int campaignId, List<PlanDto> selectedPlans)
        {
            var campaign = new CampaignDto
            {
                Id = campaignId,
                Plans = new List<PlanSummaryDto>(),
                AdvertiserMasterId = new Guid("4679219D-64C9-4773-BB91-D3BBA83E0EB7"),
                AdvertiserId = 1,
                AgencyMasterId = new Guid("C70B7C77-94EC-4373-9C09-C038E505C9CA"),
                AgencyId = 2
            };
            selectedPlans.ForEach(p => campaign.Plans.Add(
                new PlanSummaryDto
                {
                    PlanId = p.Id,
                    ProcessingStatus = PlanAggregationProcessingStatusEnum.Idle,
                    Status = p.Status,
                    PostingType = p.PostingType,
                    AudienceId = p.AudienceId
                }));
            return campaign;
        }

        private PlanDto _GetPlan(int planId, int campaignId)
        {
            return new PlanDto
            {
                Id = planId,
                CampaignId = campaignId,
                Dayparts = _GetPlanDayparts()
            };
        }

        private List<WeeklyBreakdownWeek> _GetWeeklyBreakdownWeeks(PlanDto planDto, List<int> weekIds)
        {
            List<WeeklyBreakdownWeek> result = new List<WeeklyBreakdownWeek>();
            foreach (int id in weekIds)
            {
                foreach (var c in planDto.CreativeLengths)
                {
                    foreach (var d in planDto.Dayparts)
                    {
                        result.Add(new WeeklyBreakdownWeek
                        {
                            MediaWeekId = id,
                            DaypartCodeId = d.DaypartCodeId,
                            SpotLengthId = c.SpotLengthId,
                            WeeklyAdu = 0,
                            WeeklyImpressions = 4000000,
                            WeeklyBudget = 100000.00M,
                            WeeklyRatings = 29.8892007328832,
                            UnitImpressions = 2000000
                        });
                    }
                }
            }
            return result;
        }

        private List<WeeklyBreakdownByWeek> _GetWeeklyBreakdownByWeek()
        {
            return new List<WeeklyBreakdownByWeek>
            {
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 846,
                    Impressions = 4000000,
                    Budget = 100000.00M
                },
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 847,
                    Impressions = 4000000,
                    Budget = 100000.00M
                },
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 848,
                    Impressions = 4000000,
                    Budget = 100000.00M
                },
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 849,
                    Impressions = 4000000,
                    Budget = 100000.00M
                },
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 850,
                    Impressions = 4000000,
                    Budget = 100000.00M,
                }
            };
        }

        private static List<CampaignWithSummary> _GetCampaignWithsummeriesReturn(AgencyDto agency, AdvertiserDto advertiser)
        {
            return new List<CampaignWithSummary>
            {
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 1,
                        Name = "CampaignOne",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignOne.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                    },
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 2,
                        Name = "CampaignTwo",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignTwo.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working
                    }
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 3,
                        Name = "CampaignThree",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignThree.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working
                    }
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 4,
                        Name = "Unified Campaign",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for Unified Campaign.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                        MaxFluidityPercent = 20,
                        UnifiedId = "75653",
                        UnifiedCampaignLastSentAt = new DateTime(2017,10,17),
                        UnifiedCampaignLastReceivedAt = new DateTime(2017,10,17)
                    },
                }
            };
        }

        private List<ProgramLineupProprietaryInventory> _GetProprietaryLineupData()
        {
            return new List<ProgramLineupProprietaryInventory>
            {
                new ProgramLineupProprietaryInventory{
                    Genre = "Comedy",
                    ProgramName = "The big bang theory",
                    DaypartId = 19,
                    InventoryProprietaryDaypartProgramId  = 1,
                    MarketCode = 100,
                    SpotLengthId = 1,
                    Station = new MarketCoverageByStation.Station{
                        Affiliation = "A101 Affiliation",
                        Id = 101,
                        LegacyCallLetters = "WNBC"
                    },
                    ImpressionsPerWeek = 19999
                }
            };
        }

        private DisplayDaypart _GetDisplayDaypart()
        {
            return new DisplayDaypart
            {
                Code = "PRA",
                Name = "Prime Access",
                Monday = true,
                Thursday = true,
                Sunday = true,
                StartTime = 120,
                EndTime = 45600,
                StartAMPM = "6PM-8PM"
            };
        }

        private List<PlanDaypartDto> _GetPlanDayparts()
        {
            return new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartCodeId = 10001,
                    StartTimeSeconds = 50,
                    EndTimeSeconds = 150,
                    DaypartTypeId = DaypartTypeEnum.News
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 20001,
                    StartTimeSeconds = 230,
                    EndTimeSeconds = 280,
                    DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 30002,
                    StartTimeSeconds = 350,
                    EndTimeSeconds = 450,
                    DaypartTypeId = DaypartTypeEnum.ROS
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 40001,
                    StartTimeSeconds = 100,
                    EndTimeSeconds = 200,
                    DaypartTypeId = DaypartTypeEnum.News
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 50001,
                    StartTimeSeconds = 100,
                    EndTimeSeconds = 200,
                    DaypartTypeId = DaypartTypeEnum.ROS
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 60001,
                    StartTimeSeconds = 200,
                    EndTimeSeconds = 299,
                    DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews
                }
            };
        }

        [Test]
        public void LockCampaign_ToggleOn()
        {
            // Arrange
            int campainId = 271;
            bool expectedResult = true;
            _LockingEngineMock
             .Setup(s => s.LockCampaigns(It.IsAny<int>()))
           .Returns(new BroadcastLockResponse
           {
               Key = "broadcast_campaign : 171",
               Success = true,
               LockTimeoutInSeconds = 109,
               LockedUserId = null,
               Error = null
           });
            var tc = _BuildCampaignService(false, true);
            var result = tc.LockCampaigns(campainId);
            // Assert
            Assert.AreEqual(expectedResult, result.Success);
        }

        [Test]
        public void UnlockCampaign_ToggleOn()
        {
            // Arrange
            int campainId = 271;
            bool expectedResult = true;
            _LockingEngineMock
             .Setup(s => s.UnlockCampaigns(It.IsAny<int>()))
           .Returns(new BroadcastReleaseLockResponse
           {
               Key = "broadcast_campaign : 171",
               Success = true,
               Error = null
           });
            var tc = _BuildCampaignService(false, true);
            var result = tc.UnlockCampaigns(campainId);
            // Assert
            Assert.AreEqual(expectedResult, result.Success);
        }

        [Test]
        public void GetCampaignCopy()
        {
            // Arrange
            int campaignId = 2;
            _CampaignRepositoryMock.Setup(x => x.GetCampaignCopy(It.IsAny<int>())).Returns(_GetCampaignCopy(campaignId));

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetCampaignCopy(campaignId);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private static CampaignCopyDto _GetCampaignCopy(int campaignId)
        {
            return new CampaignCopyDto
            {
                Id = 2,
                Name = "3765 campaign",
                AdvertiserMasterId = "d6dbbe81-eeb3-482d-98eb-69b24c2f7020",
                AgencyMasterId = "abca9fae-367e-4c11-8bf2-03a92412a5a7",
                Plans = new List<PlansCopyDto>
                    {
                        new PlansCopyDto
                        {
                            SourcePlanId = 341,
                            Name = "3765 CustomByWeekByDaypart",
                            ProductMasterId = "049c3450-5824-43c3-b9f6-b7158c330b61",
                            StartDate = "1/24/2022 12:00:00 AM",
                            EndDate = "1/30/2022 12:00:00 AM"
                        },
                        new PlansCopyDto
                        {
                            SourcePlanId = 342,
                            Name = "8888 CustomByWeek",
                            ProductMasterId = "049c3450-5824-43c3-b9f6-b7158c330b61",
                            StartDate = "2/24/2022 12:00:00 AM",
                            EndDate = "2/30/2022 12:00:00 AM"
                        }
                    }
            };
        }



        [Test]
        public void SaveCampaignCopy()
        {
            // Arrange
            const int campaignId = 0;
            const string campaignName = "CampaignOne";
            Guid? advertiserMasterId = Guid.NewGuid();
            Guid? agencyMasterId = Guid.NewGuid();
            var campaign = new SaveCampaignCopyDto
            {
                Name = campaignName,
                AgencyMasterId = agencyMasterId,
                AdvertiserMasterId = advertiserMasterId
            };
            _CampaignRepositoryMock
                .Setup(s => s.CheckCampaignExist(It.IsAny<SaveCampaignDto>()))
                .Returns(new CampaignDto { Id = campaignId });
            var tc = _BuildCampaignService();

            // Act
            tc.SaveCampaignCopy(campaign, _User, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<SaveCampaignDto>(c => c.Name == campaignName &&
                                        c.AdvertiserMasterId == advertiserMasterId &&
                                        campaign.AgencyMasterId == agencyMasterId),
                _User,
                _CurrentDate), Times.Once);
        }
        [Test]
        public void SaveCampaignCopy_InvalidCampaign_ThrowsException()
        {
            // Arrange
            const int campaignId = 0;
            const string expectedMessage = "This is a test exception thrown from Validate.";
            const string campaignName = "CampaignOne";
            Guid? advertiserMasterId = Guid.NewGuid();
            Guid? agencyMasterId = Guid.NewGuid();
            var campaign = new SaveCampaignCopyDto
            {
                Name = campaignName,
                AgencyMasterId = agencyMasterId,
                AdvertiserMasterId = advertiserMasterId
            };
            _CampaignRepositoryMock
                .Setup(s => s.CheckCampaignExist(It.IsAny<SaveCampaignDto>()))
                .Returns(new CampaignDto { Id = campaignId });
            _CampaignValidatorMock
                .Setup(s => s.Validate(It.IsAny<SaveCampaignDto>()))
                .Callback<SaveCampaignDto>(x => throw new Exception(expectedMessage));

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaignCopy(campaign, _User, _CurrentDate));

            // Assert
            _CampaignValidatorMock.Verify(x => x.Validate(
                It.Is<SaveCampaignDto>(c => c.Name == campaignName &&
                                        c.AdvertiserMasterId == advertiserMasterId &&
                                        campaign.AgencyMasterId == agencyMasterId)));
            _CampaignRepositoryMock.Verify(x => x.CreateCampaign(It.IsAny<SaveCampaignDto>(), _User, _CurrentDate), Times.Never);
            Assert.AreEqual(expectedMessage, caught.Message);
        }
        [Test]
        public void SaveCampaignCopy_ThrowsException()
        {
            // Arrange
            const int campaignId = 0;
            const string expectedMessage = "This is a test exception thrown from CreateCampaign.";
            const string campaignName = "CampaignOne";
            Guid? advertiserMasterId = Guid.NewGuid();
            Guid? agencyMasterId = Guid.NewGuid();
            var campaign = new SaveCampaignCopyDto
            {
                Name = campaignName,
                AgencyMasterId = agencyMasterId,
                AdvertiserMasterId = advertiserMasterId
            };
            _CampaignRepositoryMock
               .Setup(s => s.CheckCampaignExist(It.IsAny<SaveCampaignDto>()))
               .Returns(new CampaignDto { Id = campaignId });

            _CampaignRepositoryMock
                .Setup(s => s.CreateCampaign(It.IsAny<SaveCampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => throw new Exception(expectedMessage));

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaignCopy(campaign, _User, _CurrentDate));

            // Assert
            _CampaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<SaveCampaignDto>(c => c.Name == campaignName &&
                                        c.AdvertiserMasterId == advertiserMasterId &&
                                        campaign.AgencyMasterId == agencyMasterId),
                _User,
                _CurrentDate));
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void SaveCampaignCopy_ValidatesCampaign()
        {
            // Arrange
            const string campaignName = "CampaignOne";
            Guid? advertiserMasterId = Guid.NewGuid();
            Guid? agencyMasterId = Guid.NewGuid();
            var campaign = new SaveCampaignCopyDto
            {
                Name = campaignName,
                AgencyMasterId = agencyMasterId,
                AdvertiserMasterId = advertiserMasterId
            };
            _CampaignRepositoryMock
                .Setup(s => s.CheckCampaignExist(It.IsAny<SaveCampaignDto>()))
                .Returns(new CampaignDto { Id = 0 });

            var tc = _BuildCampaignService();

            // Act
            tc.SaveCampaignCopy(campaign, _User, _CurrentDate);

            // Assert
            _CampaignValidatorMock.Verify(x => x.Validate(
                It.Is<SaveCampaignDto>(c => c.Name == campaignName &&
                                        c.AdvertiserMasterId == advertiserMasterId &&
                                        campaign.AgencyMasterId == agencyMasterId)));
        }

        [Test]
        public void SaveCampaignCopy_CampaignExist()
        {
            // Arrange
            const int campaignId = 1;
            const string expectedMessage = "The Campaign CampaignOne already exists.";
            const string campaignName = "CampaignOne";
            Guid? advertiserMasterId = Guid.NewGuid();
            Guid? agencyMasterId = Guid.NewGuid();
            var campaign = new SaveCampaignCopyDto
            {
                Name = campaignName,
                AgencyMasterId = agencyMasterId,
                AdvertiserMasterId = advertiserMasterId
            };
            _CampaignRepositoryMock
                .Setup(s => s.CheckCampaignExist(It.IsAny<SaveCampaignDto>()))
                .Returns(new CampaignDto { Id = campaignId });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<CadentException>(() => tc.SaveCampaignCopy(campaign, _User, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CampaignExportAvailablePlans_WithPlans()
        {
            // Arrange
            int campaignId = 2;
            _CampaignRepositoryMock.Setup(x => x.GetCampaignPlanForExport(It.IsAny<int>())).Returns(_GetCampaignExport(campaignId));
            var tc = _BuildCampaignService();
            // Act
            var result = tc.CampaignExportAvailablePlans(campaignId);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        private static CampaignExportDto _GetCampaignExport(int campaignId)
        {
            return new CampaignExportDto
            {
                Id = 2,
                HasPlans = true,
                Plans = new List<PlanExportSummaryDto>
                    {
                        new PlanExportSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2354,
                            Status = (PlanStatusEnum)5,
                            DraftId = 2354,
                            IsDraft = false,
                            VersionNumber = 4
                        },
                        new PlanExportSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2355,
                            Status = (PlanStatusEnum)5,
                            DraftId = 0,
                            IsDraft = true,
                            VersionNumber = 4
                        },
                        new PlanExportSummaryDto
                        {
                            PlanId = 575,
                            Name = "Plan-2002",
                            VersionId = 2278,
                            Status = (PlanStatusEnum)1,
                            DraftId = 2278,
                            IsDraft =false,
                            VersionNumber = 1
                        },
                        new PlanExportSummaryDto
                        {
                            PlanId = 575,
                            Name = "Plan-2002",
                            VersionId = 2347,
                            Status = (PlanStatusEnum)5,
                            DraftId = 2347,
                            IsDraft =false,
                            VersionNumber = 2
                        }
                    }
            };
        }

        [Test]
        public void CampaignExportAvailablePlans_NoPlans()
        {
            // Arrange
            int campaignId = 2;
            _CampaignRepositoryMock.Setup(x => x.GetCampaignPlanForExport(It.IsAny<int>())).Returns(_GetCampaignWithNoPlanExport(campaignId));
            var tc = _BuildCampaignService();
            // Act
            var result = tc.CampaignExportAvailablePlans(campaignId);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        private static CampaignExportDto _GetCampaignWithNoPlanExport(int campaignId)
        {
            return new CampaignExportDto
            {
                Id = 2,
                HasPlans = false,
                Plans = new List<PlanExportSummaryDto>
                {
                }
            };
        }

        [Test]
        public void CampaignExportAvailablePlans_ThrowsException()
        {
            // Arrange
            int campaignId = 2;
            const string expectedMessage = "This is a test exception thrown from GetFilteredPlanwithdraftExcluded.";
            _CampaignRepositoryMock.Setup(x => x.GetCampaignPlanForExport(It.IsAny<int>())).
                Returns(_GetCampaignWithNoPlanExport(campaignId))
                .Callback(() => throw new Exception(expectedMessage));
            var tc = _BuildCampaignService();
            // Act
            var caught = Assert.Throws<Exception>(() => tc.CampaignExportAvailablePlans(campaignId));
            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignPlanForExport(
               It.IsAny<int>()));
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetCampaignCopy_WithPlans()
        {
            // Arrange
            int campaignId = 2;
            _CampaignRepositoryMock.Setup(x => x.GetCampaignCopy(It.IsAny<int>())).Returns(_GetCampaignCopyWithPlans(campaignId));
            var tc = _BuildCampaignService();
            // Act
            var result = tc.GetCampaignCopy(campaignId);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        private static CampaignCopyDto _GetCampaignCopyWithPlans(int campaignId)
        {
            return new CampaignCopyDto
            {
                Id = 2,
                Name = "PS - Campaign",
                AdvertiserMasterId = "20622E29-96DA-4F94-870D-AD83CC2E1335",
                AgencyMasterId = "68169E0A-98E8-4471-9AFD-37F71946E923",
                Plans = new List<PlansCopyDto>
                    {
                        new PlansCopyDto
                        {
                            SourcePlanId = 574,
                            Name = "PS-2001",
                            ProductMasterId = "D54EEDA7-DE7A-4866-8A5D-5C1B6176005B",
                            StartDate = "05/22/2022 12:00:00 AM",
                            EndDate = "06/26/2022 12:00:00 AM",
                            VersionId = 2354,
                            IsDraft = false,
                            LatestVersionId =2355
                        },
                        new PlansCopyDto
                        {
                           SourcePlanId = 575,
                            Name = "plan-2002",
                            ProductMasterId = "D54EEDA7-DE7A-4866-8A5D-5C1B6176005B",
                            StartDate = "05/02/2022 12:00:00 AM",
                            EndDate = "06/26/2022 12:00:00 AM",
                            VersionId = 2347,
                            IsDraft = false,
                            LatestVersionId =2347
                        },
                        new PlansCopyDto
                        {
                           SourcePlanId = 578,
                            Name = "2004",
                            ProductMasterId = "D54EEDA7-DE7A-4866-8A5D-5C1B6176005B",
                            StartDate = "05/02/2022 12:00:00 AM",
                            EndDate = "06/26/2022 12:00:00 AM",
                            VersionId = 2350,
                            IsDraft = false,
                            LatestVersionId =2356
                        },
                        new PlansCopyDto
                        {
                           SourcePlanId = 604,
                            Name = "Srikanth",
                            ProductMasterId = null,
                            StartDate = "05/02/2022 12:00:00 AM",
                            EndDate = "06/26/2022 12:00:00 AM",
                            VersionId = 2392,
                            IsDraft = true,
                            LatestVersionId = 2392
                        }
                    }
            };
        }

        [Test]
        public void GetCampaignCopy_NoPlans()
        {
            // Arrange
            int campaignId = 2;
            _CampaignRepositoryMock.Setup(x => x.GetCampaignCopy(It.IsAny<int>())).Returns(_GetCampaignCopyWithNoPlans(campaignId));
            var tc = _BuildCampaignService();
            // Act
            var result = tc.GetCampaignCopy(campaignId);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        private static CampaignCopyDto _GetCampaignCopyWithNoPlans(int campaignId)
        {
            return new CampaignCopyDto
            {
                Id = 2,
                Name = "PS - Campaign",
                AdvertiserMasterId = "20622E29-96DA-4F94-870D-AD83CC2E1335",
                AgencyMasterId = "68169E0A-98E8-4471-9AFD-37F71946E923",
                Plans =null
            };
        }

        [Test]
        public async Task PublishUnifiedCampaign()
        {
            // Arrange
            int campaignId = 2;
            var updateddBy = "testUser";
            var updatedDate = DateTime.Now;
            string unifiedId = "5a1521f7-6f1c-435e-90cf-bfc2816f557c";
            var messageResponse = _GetPublishUnifiedCampaign();
            _CampaignServiceApiClientMock.SetupSequence(x => x.NotifyCampaignAsync(It.IsAny<string>())).ReturnsAsync(messageResponse);
            _CampaignRepositoryMock.Setup(x => x.GetCampaign(It.IsAny<int>())).Returns(new CampaignDto
            {
                Id = campaignId,
                AdvertiserMasterId = new Guid("137B64C4-4887-4C8E-85E0-239F08609460"),
                UnifiedId = unifiedId
            });
            _CampaignRepositoryMock.Setup(x => x.UpdateUnifiedCampaignLastSentAt(It.IsAny<int>(), It.IsAny<DateTime>()));
            _CampaignRepositoryMock.Setup(x => x.UpdateCampaignLastModified(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string>()));
            var tc = _BuildCampaignService();
            // Act
            var result = await tc.PublishUnifiedCampaign(campaignId,updateddBy,updatedDate);
            // Assert            
            Assert.AreEqual(messageResponse, result);
        }
        private  static UnifiedCampaignResponseDto _GetPublishUnifiedCampaign()
        {
            return new UnifiedCampaignResponseDto
            {
                 Success =true,
                 Message = ""
            };
        }

        [Test]
        public void PublishUnifiedCampaign_ThrowsException()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from PublishUnifiedCampaign";

            int campaignId = 2;
            var updateddBy = "testUser";
            var updatedDate = DateTime.Now;
            var messageResponse = _GetPublishUnifiedCampaign();
            _CampaignServiceApiClientMock.SetupSequence(x => x.NotifyCampaignAsync(It.IsAny<string>())).ReturnsAsync(messageResponse);
            _CampaignRepositoryMock.Setup(x => x.GetCampaign(It.IsAny<int>())).Callback(() => throw new Exception(expectedMessage));
            _CampaignRepositoryMock.Setup(x => x.UpdateUnifiedCampaignLastSentAt(It.IsAny<int>(), It.IsAny<DateTime>()))
                .Callback(()=> throw new Exception(expectedMessage));
            _CampaignRepositoryMock.Setup(x => x.UpdateCampaignLastModified(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback(() => throw new Exception(expectedMessage)); 
            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(async () => await tc.PublishUnifiedCampaign(campaignId,updateddBy,updatedDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetCampaignCopy_ThrowsException()
        {
            // Arrange
            int campaignId = 2;
            const string expectedMessage = "This is a test exception thrown from GetCampaignCopy.";
            _CampaignRepositoryMock.Setup(x => x.GetCampaignCopy(It.IsAny<int>())).
                Returns(_GetCampaignCopyWithPlans(campaignId)).Callback(() => throw new Exception(expectedMessage));
            var tc = _BuildCampaignService();
            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetCampaignCopy(campaignId));
            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetCampaign_PlanStatusesWithDrafts()
        {
            // Arrange
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Name2", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };
            
            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithPlanStatusesWithDraftsReturns(agency, advertiser));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29)
                , new DateTime(2009, 3, 29)));

            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetCampaigns(new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Year = 2019,
                    Quarter = 2
                },
                PlanStatus = PlanStatusEnum.ClientApproval
            }, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        private static List<CampaignWithSummary> _GetCampaignWithPlanStatusesWithDraftsReturns(AgencyDto agency, AdvertiserDto advertiser)
        {
            return new List<CampaignWithSummary>
            {
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 240,
                        Name = "PS - Campaign",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignOne.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                        HasPlans = true,
                        PlanStatuses = new List<PlansStatusCountDto>
                        {
                            new PlansStatusCountDto
                            {
                                 PlanStatus =(PlanStatusEnum)1,
                                 Count =1
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)6,
                                Count =1
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)4,
                                Count =3
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)5,
                                Count =2
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)6,
                                Count =1
                            },

                        },
                        Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2354,
                            Status = (PlanStatusEnum)5,
                            DraftId = 2354,
                            IsDraft = false,
                            VersionNumber = 4
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2355,
                            Status = (PlanStatusEnum)5,
                            DraftId = 0,
                            IsDraft = true,
                            VersionNumber = 4
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 575,
                            Name = "Plan-2002",
                            VersionId = 2278,
                            Status = (PlanStatusEnum)1,
                            DraftId = 2278,
                            IsDraft =false,
                            VersionNumber = 1
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 607,
                            Name = "BP-4421",
                            VersionId = 2404,
                            Status = (PlanStatusEnum)4,
                            DraftId = 2404,
                            IsDraft =true,
                            VersionNumber = null
                        }
                    },
                       
                    },
                    CampaignSummary = new CampaignSummaryDto
                    {
                        CampaignId = 240,
                        PlanStatusCountWorking =3,
                        PlanStatusCountReserved =1,
                        PlanStatusCountClientApproval=0,
                        PlanStatusCountContracted= 3,
                        PlanStatusCountLive = 2,
                        PlanStatusCountComplete=1,
                        PlanStatusCountCanceled = 0,
                        PlanStatusCountRejected=0
                    }
                   
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 242,
                        Name = "ST_04-05",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignTwo.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                        HasPlans = true,
                         PlanStatuses = new List<PlansStatusCountDto>
                        {
                            new PlansStatusCountDto
                            {
                                 PlanStatus =(PlanStatusEnum)1,
                                 Count =1
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)6,
                                Count =2
                            }
                        },
                        Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = 585,
                            Name = "BP-4421",
                            VersionId = 2402,
                            Status = (PlanStatusEnum)6,
                            DraftId = 2402,
                            IsDraft = false,
                            VersionNumber = 14
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 585,
                            Name = "BP-4421",
                            VersionId = 2403,
                            Status = (PlanStatusEnum)6,
                            DraftId = 2403,
                            IsDraft = true,
                            VersionNumber = 15
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 606,
                            Name = "pankaj-001",
                            VersionId = 2395,
                            Status = (PlanStatusEnum)1,
                            DraftId = 0,
                            IsDraft =true,
                            VersionNumber = null
                        }
                    }
                    },
                     CampaignSummary = new CampaignSummaryDto
                    {
                        CampaignId = 242,
                        PlanStatusCountWorking =1,
                        PlanStatusCountReserved =0,
                        PlanStatusCountClientApproval=0,
                        PlanStatusCountContracted= 0,
                        PlanStatusCountLive = 0,
                        PlanStatusCountComplete=6,
                        PlanStatusCountCanceled = 0,
                        PlanStatusCountRejected=0
                    }
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 3,
                        Name = "CampaignThree",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignThree.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                        HasPlans = false
                    }
                }
            };
        }
        [Test]
        public void GetCampaign_PlanStatusesWithoutDrafts()
        {
            // Arrange
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Name2", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithPlanStatusesWithoutDraftsReturns(agency, advertiser));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29)
                , new DateTime(2009, 3, 29)));

            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetCampaigns(new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Year = 2019,
                    Quarter = 2
                },
                PlanStatus = PlanStatusEnum.ClientApproval
            }, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        private static List<CampaignWithSummary> _GetCampaignWithPlanStatusesWithoutDraftsReturns(AgencyDto agency, AdvertiserDto advertiser)
        {
            return new List<CampaignWithSummary>
            {
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 240,
                        Name = "PS - Campaign",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignOne.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                        HasPlans = true,
                        PlanStatuses = new List<PlansStatusCountDto>
                        {
                            new PlansStatusCountDto
                            {
                                 PlanStatus =(PlanStatusEnum)1,
                                 Count =1
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)6,
                                Count =1
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)4,
                                Count =3
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)5,
                                Count =2
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)6,
                                Count =1
                            },

                        },
                        Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2354,
                            Status = (PlanStatusEnum)5,
                            DraftId = 2354,
                            IsDraft = false,
                            VersionNumber = 4
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 574,
                            Name = "PS-2001",
                            VersionId = 2355,
                            Status = (PlanStatusEnum)5,
                            DraftId = 0,
                            IsDraft = true,
                            VersionNumber = 4
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 575,
                            Name = "Plan-2002",
                            VersionId = 2278,
                            Status = (PlanStatusEnum)1,
                            DraftId = 2278,
                            IsDraft =false,
                            VersionNumber = 1
                        }
                    }
                    },
                    CampaignSummary = new CampaignSummaryDto
                    {
                        CampaignId = 240,
                        PlanStatusCountWorking =3,
                        PlanStatusCountReserved =1,
                        PlanStatusCountClientApproval=0,
                        PlanStatusCountContracted= 3,
                        PlanStatusCountLive = 2,
                        PlanStatusCountComplete=1,
                        PlanStatusCountCanceled = 0,
                        PlanStatusCountRejected=0
                    }
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 242,
                        Name = "ST_04-05",
                        AgencyId = agency.Id,
                        AgencyMasterId = agency.MasterId,
                        AdvertiserId = advertiser.Id,
                        AdvertiserMasterId = advertiser.MasterId,
                        Notes = "Notes for CampaignTwo.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working,
                        HasPlans = true,
                         PlanStatuses = new List<PlansStatusCountDto>
                        {
                            new PlansStatusCountDto
                            {
                                 PlanStatus =(PlanStatusEnum)1,
                                 Count =1
                            },
                            new PlansStatusCountDto
                            {
                                PlanStatus = (PlanStatusEnum)6,
                                Count =2
                            }
                        },
                        Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = 585,
                            Name = "BP-4421",
                            VersionId = 2402,
                            Status = (PlanStatusEnum)6,
                            DraftId = 2402,
                            IsDraft = false,
                            VersionNumber = 14
                        },
                        new PlanSummaryDto
                        {
                            PlanId = 606,
                            Name = "pankaj-001",
                            VersionId = 2395,
                            Status = (PlanStatusEnum)1,
                            DraftId = 0,
                            IsDraft =true,
                            VersionNumber = null
                        }
                    }
                    }
                    ,
                     CampaignSummary = new CampaignSummaryDto
                    {
                        CampaignId = 242,
                        PlanStatusCountWorking =1,
                        PlanStatusCountReserved =0,
                        PlanStatusCountClientApproval=0,
                        PlanStatusCountContracted= 0,
                        PlanStatusCountLive = 0,
                        PlanStatusCountComplete=6,
                        PlanStatusCountCanceled = 0,
                        PlanStatusCountRejected=0
                    }
                },

            };
        }
        [Test]
        public void GetCampaignWithPlanStatusesWithDrafts_ThrowsException()
        {
            const string expectedMessage = "This is a test exception thrown from GetCampaigns.";
            // Arrange
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Name2", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };
            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithPlanStatusesWithDraftsReturns(agency, advertiser))
                .Callback(() => throw new Exception(expectedMessage)); 
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29)
                , new DateTime(2009, 3, 29))).Callback(() => throw new Exception(expectedMessage)); 
            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency).Callback(() => throw new Exception(expectedMessage));
            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser).Callback(() => throw new Exception(expectedMessage)); 
            var tc = _BuildCampaignService();
            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetCampaigns(new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Year = 2019,
                    Quarter = 2
                },
                PlanStatus = PlanStatusEnum.ClientApproval
            }, _CurrentDate));
            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>()), Times.Once);
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ProgramLineup_WithCustomDaypart()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b");
            Guid advertiserId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 50001,
                            StartTimeSeconds = 39600,
                            EndTimeSeconds = 46799,
                            DaypartTypeId = DaypartTypeEnum.Sports
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 60001,
                            StartTimeSeconds = 72000,
                            EndTimeSeconds = 299,
                            DaypartTypeId = DaypartTypeEnum.Sports
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 30001,
                            StartTimeSeconds = 57600,
                            EndTimeSeconds = 68399,
                            DaypartTypeId = DaypartTypeEnum.News
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 10001,
                            StartTimeSeconds = 14400,
                            EndTimeSeconds = 35999,
                            DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 20001,
                            StartTimeSeconds = 39600,
                            EndTimeSeconds = 46799,
                            DaypartTypeId = DaypartTypeEnum.ROS
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 40001,
                            StartTimeSeconds = 72000,
                            EndTimeSeconds = 299,
                            DaypartTypeId = DaypartTypeEnum.News
                        }
                    },
                    PricingParameters = new PlanPricingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });

            _PlanRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupForRollupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPricingJobForLatestPlanVersion(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 4001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        [Test]
        public void ReturnsFilteredUnifiedCampaigns_WithToggleOn()
        {
            // Arrange
         
            var agency = new AgencyDto { Id = 1, MasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B"), Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, MasterId = new Guid("1806450A-E0A3-416D-B38D-913FB5CF3879"), Name = "Name2", AgencyId = 1, AgencyMasterId = new Guid("89AB30C5-23A7-41C1-9B7D-F5D9B41DBE8B") };

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithsummeriesReturn(agency, advertiser));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29)
                , new DateTime(2009, 3, 29)));

            _AabEngine.Setup(x => x.GetAgency(It.IsAny<Guid>()))
                .Returns(agency);

            _AabEngine.Setup(x => x.GetAdvertiser(It.IsAny<Guid>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_UNIFIED_CAMPAIGN] = true;
            // Act
            var result = tc.GetCampaigns(new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Year = 2019,
                    Quarter = 2
                },
                PlanStatus = PlanStatusEnum.ClientApproval
            }, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ProgramLineup_WithToggleOn()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            Guid agencyId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b");
            Guid advertiserId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F");
            const int audienceId = 6;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId },

            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    CreativeLengths = new List<CreativeLength> {
                        new CreativeLength { SpotLengthId = 1, Weight = 50 },
                        new CreativeLength { SpotLengthId = 2, Weight = 20 },
                        new CreativeLength { SpotLengthId = 3, Weight = 30 }
                    },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts(),
                    PricingParameters = new PlanPricingParametersDto { JobId = 1 },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek { NumberOfActiveDays = 7 } }
                });
            _PlanRepositoryMock
                .Setup(x => x.GetProprietaryInventoryForProgramLineup(It.IsAny<int>()))
                .Returns(_GetProprietaryLineupData());

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyMasterId = agencyId,
                    AdvertiserMasterId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _DaypartCacheMock.Setup(x => x.GetDisplayDaypart(It.IsAny<int>()))
                .Returns(_GetDisplayDaypart());

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();
            _LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_PROGRAM_LINEUP_ALLOCATION_BY_AFFILIATE] = true;
            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPricingJobForLatestPlanVersion(firstPlanId), Times.Once);
            _AabEngine.Verify(x => x.GetAgency(agencyId), Times.Once);
            _AabEngine.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthRepositoryMock.Verify(x => x.GetSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId, It.IsAny<PostingTypeEnum>(), It.IsAny<SpotAllocationModelMode>()), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetCampaignWithDefaults()
        {
            // Arrange
            int campaignId = 2;
            _CampaignRepositoryMock.Setup(x => x.GetCampaignWithDefaults(It.IsAny<int>())).Returns(_GetCampaignWithDefaults(campaignId));
            var tc = _BuildCampaignService();
            // Act
            var result = tc.GetCampaignWithDefaults(campaignId);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private static CampaignWithDefaultsDto _GetCampaignWithDefaults(int campaignId)
        {
            return new CampaignWithDefaultsDto
            {
                SecondaryAudiences = new List<CampaignPlanSecondaryAudiencesDto>
                    {
                        new CampaignPlanSecondaryAudiencesDto
                        {
                            Type = AudienceTypeEnum.Nielsen,
                            AudienceId = 35
                        },
                        new CampaignPlanSecondaryAudiencesDto
                        {
                            Type = AudienceTypeEnum.Nielsen,
                            AudienceId = 39
                        }
                    },
                MaxFluidityPercent = 20
            };
        }
    }
}
