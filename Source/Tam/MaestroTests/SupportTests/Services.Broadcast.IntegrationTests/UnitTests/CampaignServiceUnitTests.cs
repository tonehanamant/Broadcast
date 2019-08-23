using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Campaigns;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Campaigns;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    public class CampaignServiceUnitTests
    {
        #region Constructor

        [Test]
        public void ConstructorTest()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();

            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine.Object);

            Assert.IsNotNull(tc);
        }

        #endregion // #region Constructor

        #region GetCampaigns

        [Test]
        public void GetCampaigns()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine);
            var campaignData = new Mock<ICampaignServiceData>();
            var getAllCampaignsCallCount = 0;
            var getAllCampaignsReturn = new List<CampaignDto>
            {
                new CampaignDto{Id = 1, Name = "CampaignOne", AgencyId = 1, AdvertiserId = 1, Notes = "Notes for CampaignOne.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)},
                new CampaignDto{Id = 2, Name = "CampaignTwo", AgencyId = 2, AdvertiserId = 2, Notes = "Notes for CampaignTwo.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)},
                new CampaignDto{Id = 3, Name = "CampaignThree", AgencyId = 3, AdvertiserId = 3, Notes = "Notes for CampaignThree.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)}
            };
            var filter = new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Quarter = 3,
                    Year = 2019
                }
            };
            campaignData.Setup(s => s.GetCampaigns(It.Is<QuarterDetailDto>(p => p.Quarter == 3 && p.Year == 2019)))
                .Callback(() => getAllCampaignsCallCount++)
                .Returns(getAllCampaignsReturn);
            tc.CampaignServiceData = campaignData.Object;

            var items = tc.GetCampaigns(filter, new DateTime(2019, 04, 01));

            Assert.AreEqual(1, getAllCampaignsCallCount);
            Assert.IsNotNull(items);
            Assert.AreEqual(getAllCampaignsReturn.Count, items.Count);
        }

        [Test]
        public void GetCampaignsWithException()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine);
            var campaignData = new Mock<ICampaignServiceData>();
            var getAllCampaignsCallCount = 0;
            var getAllCampaignsReturn = new List<CampaignDto>
            {
                new CampaignDto{Id = 1, Name = "CampaignOne", AgencyId = 1, AdvertiserId = 1, Notes = "Notes for CampaignOne.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)},
                new CampaignDto{Id = 2, Name = "CampaignTwo", AgencyId = 2, AdvertiserId = 2, Notes = "Notes for CampaignTwo.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)},
                new CampaignDto{Id = 3, Name = "CampaignThree", AgencyId = 3, AdvertiserId = 3, Notes = "Notes for CampaignThree.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)}
            };
            var filter = new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Quarter = 3,
                    Year = 2019
                }
            };
            campaignData.Setup(s => s.GetCampaigns(It.Is<QuarterDetailDto>(p => p.Quarter == 3 && p.Year == 2019)))
                .Callback(() =>
                {
                    getAllCampaignsCallCount++;
                    throw new Exception("This is a test exception thrown from GetAllCampaigns.");
                })
                .Returns(getAllCampaignsReturn);
            tc.CampaignServiceData = campaignData.Object;

            var caught = Assert.Throws<Exception>(() => tc.GetCampaigns(filter, new DateTime(2019, 04, 01)));

            Assert.AreEqual(1, getAllCampaignsCallCount);
            Assert.IsTrue(caught.Message.Contains("This is a test exception thrown from GetAllCampaigns."));
        }

        [Test]
        public void GetCampaignsWithFilter()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine);
            var campaignData = new Mock<ICampaignServiceData>();
            var getAllCampaignsCallCount = 0;
            var getAllCampaignsReturn = new List<CampaignDto>
            {
                new CampaignDto{Id = 1, Name = "CampaignOne", AgencyId = 1, AdvertiserId = 1, Notes = "Notes for CampaignOne.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)},
                new CampaignDto{Id = 2, Name = "CampaignTwo", AgencyId = 2, AdvertiserId = 2, Notes = "Notes for CampaignTwo.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)},
                new CampaignDto{Id = 3, Name = "CampaignThree", AgencyId = 3, AdvertiserId = 3, Notes = "Notes for CampaignThree.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)}
            };
            var filter = new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Quarter = 3,
                    Year = 2019
                }
            };

            campaignData.Setup(s => s.GetCampaigns(It.Is<QuarterDetailDto>(p => p.Quarter == 3 && p.Year == 2019)))
                .Callback(() => getAllCampaignsCallCount++)
                .Returns(getAllCampaignsReturn);
            tc.CampaignServiceData = campaignData.Object;

            var items = tc.GetCampaigns(filter, new DateTime(2019, 04, 01));

            Assert.AreEqual(1, getAllCampaignsCallCount);
            Assert.IsNotNull(items);
            Assert.AreEqual(getAllCampaignsReturn.Count, items.Count);
        }

        [Test]
        public void GetCampaignsWithNullFilter()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine);
            var campaignData = new Mock<ICampaignServiceData>();
            var getAllCampaignsReturn = new List<CampaignDto>
            {
                new CampaignDto{Id = 1, Name = "CampaignOne", AgencyId = 1, AdvertiserId = 1, Notes = "Notes for CampaignOne.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)},
                new CampaignDto{Id = 2, Name = "CampaignTwo", AgencyId = 2, AdvertiserId = 2, Notes = "Notes for CampaignTwo.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)},
                new CampaignDto{Id = 3, Name = "CampaignThree", AgencyId = 3, AdvertiserId = 3, Notes = "Notes for CampaignThree.", ModifiedBy = "TestUser", ModifiedDate = new DateTime(2017,10,17)}
            };

            campaignData.Setup(s => s.GetCampaigns(It.Is<QuarterDetailDto>(p => p.Quarter == 3 && p.Year == 2019)))
                .Returns(getAllCampaignsReturn);
            tc.CampaignServiceData = campaignData.Object;

            var items = tc.GetCampaigns(null, new DateTime(2019, 08, 01));

            campaignData.Verify(x => x.GetCampaigns(It.Is<QuarterDetailDto>(p => p.Quarter == 3 && p.Year == 2019)), Times.Once);

            Assert.IsNotNull(items);
            Assert.AreEqual(getAllCampaignsReturn.Count, items.Count);
        }

        #endregion // #region GetCampaigns

        #region GetAdvertisers

        [Test]
        public void GetAdvertisers()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine.Object);
            var campaignData = new Mock<ICampaignServiceData>();
            var getAdvertisersCallCount = 0;
            var getAdvertisersReturn = new List<AdvertiserDto>
            {
                new AdvertiserDto{Id = 1, Name = "AdvertiserOne"},
                new AdvertiserDto{Id = 2, Name = "AdvertiserTwo"},
                new AdvertiserDto{Id = 3, Name = "AdvertiserThree"}
            };
            campaignData.Setup(s => s.GetAdvertisers())
                .Callback(() => getAdvertisersCallCount++)
                .Returns(getAdvertisersReturn);
            tc.CampaignServiceData = campaignData.Object;

            var items = tc.GetAdvertisers();

            Assert.AreEqual(1, getAdvertisersCallCount);
            Assert.IsNotNull(items);
            Assert.AreEqual(getAdvertisersReturn.Count, items.Count);
        }

        [Test]
        public void GetAdvertisersWithException()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine.Object);
            var campaignData = new Mock<ICampaignServiceData>();
            var getAdvertisersCallCount = 0;
            var getAdvertisersReturn = new List<AdvertiserDto>
            {
                new AdvertiserDto{Id = 1, Name = "AdvertiserOne"},
                new AdvertiserDto{Id = 2, Name = "AdvertiserTwo"},
                new AdvertiserDto{Id = 3, Name = "AdvertiserThree"}
            };
            campaignData.Setup(s => s.GetAdvertisers())
                .Callback(() =>
                {
                    getAdvertisersCallCount++;
                    throw new Exception("This is a test exception thrown from GetAdvertisers.");
                })
                .Returns(getAdvertisersReturn);
            tc.CampaignServiceData = campaignData.Object;

            var caught = Assert.Throws<Exception>(() => tc.GetAdvertisers());

            Assert.AreEqual(1, getAdvertisersCallCount);
            Assert.IsTrue(caught.Message.Contains("This is a test exception thrown from GetAdvertisers."));
        }

        #endregion // #region GetAdvertisers

        #region GetAgencies

        [Test]
        public void GetAgencies()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine.Object);
            var campaignData = new Mock<ICampaignServiceData>();
            var getAgenciesCallCount = 0;
            var getAgenciesReturn = new List<AgencyDto>
            {
                new AgencyDto{Id = 1, Name = "AgencyOne"},
                new AgencyDto{Id = 2, Name = "AgencyTwo"},
                new AgencyDto{Id = 3, Name = "AgencyThree"}
            };
            campaignData.Setup(s => s.GetAgencies())
                .Callback(() => getAgenciesCallCount++)
                .Returns(getAgenciesReturn);
            tc.CampaignServiceData = campaignData.Object;

            var items = tc.GetAgencies();

            Assert.AreEqual(1, getAgenciesCallCount);
            Assert.IsNotNull(items);
            Assert.AreEqual(getAgenciesReturn.Count, items.Count);
        }

        [Test]
        public void GetAgenciesWithException()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine.Object);
            var campaignData = new Mock<ICampaignServiceData>();
            var getAgenciesCallCount = 0;
            var getAgenciesReturn = new List<AgencyDto>
            {
                new AgencyDto{Id = 1, Name = "AgencyOne"},
                new AgencyDto{Id = 2, Name = "AgencyTwo"},
                new AgencyDto{Id = 3, Name = "AgencyThree"}
            };
            campaignData.Setup(s => s.GetAgencies())
                .Callback(() =>
                {
                    getAgenciesCallCount++;
                    throw new Exception("This is a test exception thrown from GetAgencies.");
                })
                .Returns(getAgenciesReturn);
            tc.CampaignServiceData = campaignData.Object;

            var caught = Assert.Throws<Exception>(() => tc.GetAgencies());

            Assert.AreEqual(1, getAgenciesCallCount);
            Assert.IsTrue(caught.Message.Contains("This is a test exception thrown from GetAgencies."));
        }

        #endregion // #region GetAgencies

        #region GetQuarters

        [Test]
        public void GetQuarters()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = IntegrationTestApplicationServiceFactory.MediaMonthAndWeekAggregateCache;
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache, quarterCalculationEngine);
            var campaignData = new Mock<ICampaignServiceData>();
            var getCampaignsDateRangesReturn = new List<DateRange>
            {
                new DateRange(null, null),
                new DateRange(new DateTime(2019, 1, 1), null),
                new DateRange(new DateTime(2019, 2, 1), new DateTime(2019, 9, 1))
            };
            campaignData.Setup(s => s.GetCampaignsDateRanges())
                .Returns(getCampaignsDateRangesReturn);
            tc.CampaignServiceData = campaignData.Object;

            var campaignQuarters = tc.GetQuarters(new DateTime(2019, 8, 20));

            Assert.IsNotNull(campaignQuarters);
            Assert.AreEqual(3, campaignQuarters.Quarters.Count);
            Assert.AreEqual(3, campaignQuarters.DefaultQuarter.Quarter);
            Assert.AreEqual(2019, campaignQuarters.DefaultQuarter.Year);
        }

        [Test]
        public void GetQuartersDefaultQuarter()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = IntegrationTestApplicationServiceFactory.MediaMonthAndWeekAggregateCache;
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache, quarterCalculationEngine);
            var campaignData = new Mock<ICampaignServiceData>();
            var getCampaignsDateRangesReturn = new List<DateRange>();
            campaignData.Setup(s => s.GetCampaignsDateRanges())
                .Returns(getCampaignsDateRangesReturn);
            tc.CampaignServiceData = campaignData.Object;

            var campaignQuarters = tc.GetQuarters(new DateTime(2019, 8, 20));

            Assert.IsNotNull(campaignQuarters);
            Assert.AreEqual(1, campaignQuarters.Quarters.Count);
            Assert.AreEqual(3, campaignQuarters.DefaultQuarter.Quarter);
            Assert.AreEqual(2019, campaignQuarters.DefaultQuarter.Year);
        }

        #endregion // #region GetQuarters

        #region CreateCampaign

        [Test]
        public void CreateCampaign()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine.Object);
            var campaignValidator = new Mock<ICampaignValidator>();
            var validateCalls = new List<CampaignDto>();
            campaignValidator.Setup(s => s.Validate(It.IsAny<CampaignDto>()))
                .Callback<CampaignDto>((c) => validateCalls.Add(c));
            tc.CampaignValidator = campaignValidator.Object;
            var campaignData = new Mock<ICampaignServiceData>();
            var createCampaignCalls = new List<Tuple<CampaignDto, string, DateTime>>();
            campaignData.Setup(s => s.SaveCampaign(It.IsAny<CampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<CampaignDto, string, DateTime>((c, u, d) =>
                    createCampaignCalls.Add(new Tuple<CampaignDto, string, DateTime>(c, u, d)));
            tc.CampaignServiceData = campaignData.Object;
            var campaign = new CampaignDto
            {
                Id = 0,
                Name = "CampaignOne",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
            var createdBy = "TestUser";
            var createdDate = new DateTime(2017,10,17,7,30,23);

            tc.SaveCampaign(campaign, createdBy, createdDate);
            
            Assert.AreEqual(1, validateCalls.Count);
            Assert.AreEqual(1, createCampaignCalls.Count);
            Assert.AreEqual("CampaignOne", createCampaignCalls[0].Item1.Name);
            Assert.AreEqual(createdBy, createCampaignCalls[0].Item1.ModifiedBy);
            Assert.AreEqual(createdDate, createCampaignCalls[0].Item1.ModifiedDate);
            Assert.AreEqual(createdBy, createCampaignCalls[0].Item2);
            Assert.AreEqual(createdDate, createCampaignCalls[0].Item3);
        }

        [Test]
        public void CreateCampaignWithValidationFail()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine.Object);
            var campaignValidator = new Mock<ICampaignValidator>();
            var validateCalls = new List<CampaignDto>();
            campaignValidator.Setup(s => s.Validate(It.IsAny<CampaignDto>()))
                .Callback<CampaignDto>((c) =>
                {
                    validateCalls.Add(c);
                    throw new Exception("This is a test exception thrown from Validate.");
                });
            tc.CampaignValidator = campaignValidator.Object;
            var campaignData = new Mock<ICampaignServiceData>();
            var createCampaignCalls = new List<Tuple<CampaignDto, string, DateTime>>();
            campaignData.Setup(s => s.SaveCampaign(It.IsAny<CampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<CampaignDto, string, DateTime>((c, u, d) =>
                    createCampaignCalls.Add(new Tuple<CampaignDto, string, DateTime>(c, u, d)));
            tc.CampaignServiceData = campaignData.Object;
            var campaign = new CampaignDto
            {
                Id = 0,
                Name = "CampaignOne",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
            var createdBy = "TestUser";
            var createdDate = new DateTime(2017, 10, 17, 7, 30, 23);

            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, createdBy, createdDate));
            
            Assert.AreEqual("This is a test exception thrown from Validate.", caught.Message);
            Assert.AreEqual(0, createCampaignCalls.Count);
        }

        [Test]
        public void CreateCampaignWithSaveFail()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var smsClient = new Mock<ISMSClient>();
            var mediaAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngine = new Mock<IQuarterCalculationEngine>();
            var tc = new CampaignServiceUnitTestClass(dataRepoFactory.Object, smsClient.Object, mediaAggregateCache.Object, quarterCalculationEngine.Object);
            var campaignValidator = new Mock<ICampaignValidator>();
            var validateCalls = new List<CampaignDto>();
            campaignValidator.Setup(s => s.Validate(It.IsAny<CampaignDto>()))
                .Callback<CampaignDto>((c) => validateCalls.Add(c));
            tc.CampaignValidator = campaignValidator.Object;
            var campaignData = new Mock<ICampaignServiceData>();
            var createCampaignCalls = new List<Tuple<CampaignDto, string, DateTime>>();
            campaignData.Setup(s => s.SaveCampaign(It.IsAny<CampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<CampaignDto, string, DateTime>((c, u, d) =>
                {
                    createCampaignCalls.Add(new Tuple<CampaignDto, string, DateTime>(c, u, d));
                    throw new Exception("This is a test exception thrown from CreateCampaign.");
                });
            tc.CampaignServiceData = campaignData.Object;
            var campaign = new CampaignDto
            {
                Id = 0,
                Name = "CampaignOne",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
            var createdBy = "TestUser";
            var createdDate = new DateTime(2017, 10, 17, 7, 30, 23);

            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, createdBy, createdDate));

            Assert.AreEqual("This is a test exception thrown from CreateCampaign.", caught.Message);
            Assert.AreEqual(1, validateCalls.Count);
            Assert.AreEqual(1, createCampaignCalls.Count);
        }

        #endregion // #region CreateCampaign
    }
}