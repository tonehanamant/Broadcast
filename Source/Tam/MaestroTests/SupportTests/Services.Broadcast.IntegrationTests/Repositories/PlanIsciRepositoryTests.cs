using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class PlanIsciRepositoryTests
    {
        [Test]
       public void GetAvailableIscis()
        {
            // Arrange
            DateTime startDate = new DateTime(2021, 01, 01);
            DateTime endDate = new DateTime(2024, 08, 29);
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            //Act
            var result = repo.GetAvailableIscis(startDate, endDate);
            //Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        [Test]
        public void GetUnAvailableIscis()
        {
            // Arrange
            DateTime startDate = new DateTime(2015, 01, 01);
            DateTime endDate = new DateTime(2016, 08, 29);
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            //Act
            var result = repo.GetAvailableIscis(startDate, endDate);
            //Assert
            Assert.IsTrue(result.Count == 0);          
        }
        [Test]
        public void GetAvailableIscis_Overlap()
        {
            // Arrange
            DateTime startDate = new DateTime(2021, 07, 26);
            DateTime endDate = new DateTime(2021, 08, 29);

            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            // Act
            var result = repo.GetAvailableIscis(startDate, endDate);
            // Assert
           Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        [Test]
        public void GetAvailableIscis_OverlapOneDay()
        {
            // Arrange
            DateTime startDate = new DateTime(2020, 08, 08);
            DateTime endDate = new DateTime(2021, 01, 01);
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            // Act
            var result = repo.GetAvailableIscis(startDate, endDate);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        
        [Test]
        public void GetAvailableIsciPlans_Plan_DoesNotExist()
        {
            // Arrange
            DateTime mediaMonthStartDate = new DateTime(2015, 02, 23);
            DateTime mediaMonthEndDate = new DateTime(2015, 03, 29);
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            // Act
            var result = planIsciRepository.GetAvailableIsciPlans(mediaMonthStartDate, mediaMonthEndDate);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetAvailableIsciPlans_PlanWithIsci_Exists_OverlapStartDateEndDate()
        {
            // Arrange
            DateTime mediaMonthStartDate = new DateTime(2019, 08, 26);
            DateTime mediaMonthEndDate = new DateTime(2019, 09, 29);
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            // Act
            var result = planIsciRepository.GetAvailableIsciPlans(mediaMonthStartDate, mediaMonthEndDate);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIsciPlans_PlanWithIsci_Exists_OverlapStartDate()
        {
            // Arrange
            DateTime mediaMonthStartDate = new DateTime(2019, 07, 29);
            DateTime mediaMonthEndDate = new DateTime(2019, 08, 25);
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            // Act
            var result = planIsciRepository.GetAvailableIsciPlans(mediaMonthStartDate, mediaMonthEndDate);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAvailableIsciPlans_PlansWithoutIsci_Exist_OverlapEndDate()
        {
            // Arrange
            DateTime mediaMonthStartDate = new DateTime(2020, 03, 30);
            DateTime mediaMonthEndDate = new DateTime(2020, 04, 26);
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            // Act
            var result = planIsciRepository.GetAvailableIsciPlans(mediaMonthStartDate, mediaMonthEndDate);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}
