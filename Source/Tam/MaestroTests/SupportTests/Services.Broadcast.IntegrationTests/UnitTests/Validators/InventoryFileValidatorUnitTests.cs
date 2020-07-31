using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Validators;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.Validators
{
    public class InventoryFileValidatorUnitTests
    {
        private InventoryFileValidator _InventoryFileValidator;

        [SetUp]
        public void Setup()
        {
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();

            _InventoryFileValidator = new InventoryFileValidator(dataRepositoryFactoryMock.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void DuplicateRecordValidation()
        {
            var result = _InventoryFileValidator.DuplicateRecordValidation("AXW");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ValidateInventoryFile()
        {
            var inventoryFile = _GetInventoryFile();

            var result = _InventoryFileValidator.ValidateInventoryFile(inventoryFile);

            Assert.IsEmpty(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ValidateInventoryFile_DuplicateValue()
        {
            var inventoryFile = _GetInventoryFile();
            inventoryFile.InventoryGroups.FirstOrDefault().Manifests.Add(_GetStationInventoryManifest());

            var result = _InventoryFileValidator.ValidateInventoryFile(inventoryFile);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private StationInventoryManifest _GetStationInventoryManifest() =>
            new StationInventoryManifest
            {
                SpotLengthId = 1,
                Station = new DisplayBroadcastStation { LegacyCallLetters = "AXW" },
                ManifestDayparts = new List<StationInventoryManifestDaypart>()
                                   {
                                       new StationInventoryManifestDaypart
                                       {
                                           Daypart = new DisplayDaypart() { Code = "EMN", Name = "Early Morning News" }
                                       }

                                   }
            };

        private InventoryFile _GetInventoryFile() =>
            new InventoryFile()
            {
                InventoryGroups = new List<StationInventoryGroup>
                {
                     new StationInventoryGroup
                     {
                           Manifests = new List<StationInventoryManifest>
                           {
                               _GetStationInventoryManifest()
                           },
                     },
                     new StationInventoryGroup
                     {
                           Manifests = new List<StationInventoryManifest>
                           {
                               new StationInventoryManifest
                               {
                                   SpotLengthId = 2,
                                   Station = new DisplayBroadcastStation { LegacyCallLetters = "MSN"},
                                   ManifestDayparts = new List<StationInventoryManifestDaypart>()
                                   {
                                       new StationInventoryManifestDaypart
                                       {
                                           Daypart = new DisplayDaypart() { Code = "EN", Name = "Evening News" }
                                       }

                                   }
                               },

                           },
                     }
                 }
            };
    }
}
