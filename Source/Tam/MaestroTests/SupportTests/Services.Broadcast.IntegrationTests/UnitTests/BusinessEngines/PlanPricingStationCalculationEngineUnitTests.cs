﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    public class PlanPricingStationCalculationEngineUnitTests
    {
        private PlanPricingStationCalculationEngine _PlanPricingStationCalculationEngine;
        private Mock<IMarketRepository> _MarketRepository;
        private Mock<IStationRepository> _StationRepository;

        [SetUp]
        public void Setup()
        {
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _MarketRepository = new Mock<IMarketRepository>();
            _StationRepository = new Mock<IStationRepository>();

            dataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IMarketRepository>())
                .Returns(_MarketRepository.Object);

            dataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IStationRepository>())
                .Returns(_StationRepository.Object);

            _PlanPricingStationCalculationEngine = new PlanPricingStationCalculationEngine(dataRepositoryFactoryMock.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_Success()
        {
            _StationRepository.Setup(s => s.GetLatestStationMonthDetailsForStations(It.IsAny<List<int>>())).Returns(_GetStationMonthDetails());

            _MarketRepository.Setup(m => m.GetMarket(100)).Returns(new Tam.Maestro.Data.Entities.DataTransferObjects.LookupDto(100, "New York"));
            _MarketRepository.Setup(m => m.GetMarket(200)).Returns(new Tam.Maestro.Data.Entities.DataTransferObjects.LookupDto(200, "Las Vegas"));
            _MarketRepository.Setup(m => m.GetMarket(300)).Returns(new Tam.Maestro.Data.Entities.DataTransferObjects.LookupDto(300, "Seattle"));

            var planPricingParameters = _GetPlanPricingParametersDto();
            var allocationResult = _GetPlanPricingAllocationResult();
            var inventory = _GetPlanPricingInventoryPrograms();

            var result = _PlanPricingStationCalculationEngine.Calculate(inventory, allocationResult, planPricingParameters);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<StationMonthDetailDto> _GetStationMonthDetails() =>
            new List<StationMonthDetailDto>
            {
                new StationMonthDetailDto
                {
                    StationId = 30,
                    MarketCode = 100
                },
                new StationMonthDetailDto
                {
                    StationId = 27,
                    MarketCode = 200
                },
                new StationMonthDetailDto
                {
                    StationId = 99,
                    MarketCode = 300
                },
            };

        private PlanPricingParametersDto _GetPlanPricingParametersDto() =>
            new PlanPricingParametersDto
            {
                PlanVersionId = 15,
                JobId = 51
            };

        private PlanPricingAllocationResult _GetPlanPricingAllocationResult() =>
            new PlanPricingAllocationResult
            {
                Spots = new List<PlanPricingAllocatedSpot>
                {
                    new PlanPricingAllocatedSpot
                    {
                         Id = 5,
                         SpotCost = 15.5m,
                         Spots = 5,
                         Impressions = 500
                    },
                    new PlanPricingAllocatedSpot
                    {
                        Id = 20,
                         SpotCost = 9.99m,
                         Spots = 3,
                         Impressions = 750
                    }
                }
            };

        private List<PlanPricingInventoryProgram> _GetPlanPricingInventoryPrograms() =>
            new List<PlanPricingInventoryProgram>
            {
                new PlanPricingInventoryProgram
                {
                     ManifestId = 5,
                     Station = new Entities.DisplayBroadcastStation
                     {
                          Id = 30,
                          LegacyCallLetters = "KOB",
                     }
                },
                new PlanPricingInventoryProgram
                {
                     ManifestId = 20,
                     Station = new Entities.DisplayBroadcastStation
                     {
                          Id = 27,
                          LegacyCallLetters = "KOTA",
                     }
                },
                new PlanPricingInventoryProgram
                {
                     ManifestId = 5,
                     Station = new Entities.DisplayBroadcastStation
                     {
                          Id = 30,
                          LegacyCallLetters = "KOB",
                     }
                },
                new PlanPricingInventoryProgram
                {
                     ManifestId = 6,
                     Station = new Entities.DisplayBroadcastStation
                     {
                          Id = 99,
                          LegacyCallLetters = "TVX",
                     }
                },
            };
    }
}
