using NUnit.Framework;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))]
    public class ReelIsciProductRepositoryTests
    {
        private IReelIsciProductRepository _ReelIsciProductRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IReelIsciProductRepository>();

        [Test]
        public void DeleteReelIsciProductsNotExistInReelIsci()
        {
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
                var addedCount = _ReelIsciProductRepository.SaveIsciProductMappings(isciMappings.IsciProductMappings, createdBy, createdAt);
                result = _ReelIsciProductRepository.DeleteReelIsciProductsNotExistInReelIsci();
            }

            // Assert
            Assert.IsTrue(result >= expectedDeleteCount);
        }

        [Test]
        public void SaveIsciProductMappings()
        {
            // Arrange
            var createdBy = "Test User";
            var createdAt = DateTime.Now;
            var isciProductMappings = new List<IsciProductMappingDto>
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
            };
            var iscis = isciProductMappings.Select(s => s.Isci).Distinct().ToList();

            int result = 0;
            List<IsciProductMappingDto> saveResult;
            // Act
            using (new TransactionScopeWrapper())
            {
                result = _ReelIsciProductRepository.SaveIsciProductMappings(isciProductMappings, createdBy, createdAt);
                saveResult = _ReelIsciProductRepository.GetIsciProductMappings(iscis);
            }
            Assert.AreEqual(2, result);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(IsciProductMappingDto), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(saveResult, jsonSettings));
        }
    }
}
