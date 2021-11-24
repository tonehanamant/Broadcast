using NUnit.Framework;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    public class ReelIsciProductRepositoryTests
    {
        [Test]
        public void DeleteReelIsciProductsNotExistInReelIsci()
        {
            var reelIsciProductRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciProductRepository>();
            var planIsciRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();
            var createdBy = "Test User";
            var createdAt = DateTime.Now;
            var isciMappings = new IsciPlanMappingsSaveRequestDto()
            {
                IsciProductMappings = new List<IsciProductMappingDto>
                {
                    new IsciProductMappingDto()
                    {
                        ProductName = "Femoston",
                        Isci= "UniqueIsci1"
                    },
                    new IsciProductMappingDto()
                    {
                        ProductName = "Abbot Labs",
                        Isci= "UniqueIsci2"
                    }
                }
            };
            var result = 0;
            var expectedDeleteCount = 2;

            // Act
            using (new TransactionScopeWrapper())
            {
                var addedCount = planIsciRepository.SaveIsciProductMappings(isciMappings.IsciProductMappings, createdBy, createdAt);
                result = reelIsciProductRepository.DeleteReelIsciProductsNotExistInReelIsci();
            }

            // Assert
            Assert.IsTrue(result >= expectedDeleteCount);
        }
    }
}
