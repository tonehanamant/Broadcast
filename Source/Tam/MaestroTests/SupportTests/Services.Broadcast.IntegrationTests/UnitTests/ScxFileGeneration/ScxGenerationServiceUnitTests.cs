using Common.Services;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Scx;
using Services.Broadcast.Entities.Scx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.ScxFileGeneration
{
    public class ScxGenerationServiceUnitTests
    {
        #region Constructor

        [Test]
        public void ConstructorTest()
        {
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();

            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object, 
                proprietaryInventoryService.Object,
                fileService.Object);

            Assert.IsNotNull(tc);
        }

        #endregion // #region Constructor

        #region GetScxFileGenerationHistory

        [Test]
        public void GetScxFileGenerationHistoryHappyPath()
        {
            var sourceId = 7;
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var dropFolder = "thisFolder";
            var historian = new Mock<IScxFileGenerationHistorian>();
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetail>
            {
                new ScxFileGenerationDetail(),
                new ScxFileGenerationDetail(),
                new ScxFileGenerationDetail()
            };
            historian.Setup(s => s.GetScxFileGenerationHistory(It.IsAny<int>()))
                .Callback<int>((s) => getHistoryCalls.Add(s))
                .Returns(getHistoryReturn);
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object,
                fileService.Object)
            {
                DropFolderPath = dropFolder,
                ScxFileGenerationHistorian = historian.Object
            };

            var result = tc.GetScxFileGenerationHistory(sourceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, getHistoryCalls.Count);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void GetScxFileGenerationHistoryWithException()
        {
            var sourceId = 7;
            var dataRepoFactory = new Mock<IDataRepositoryFactory>();
            var proprietaryInventoryService = new Mock<IProprietaryInventoryService>();
            var fileService = new Mock<IFileService>();
            var dropFolder = "thisFolder";
            var historian = new Mock<IScxFileGenerationHistorian>();
            var getHistoryCalls = new List<int>();
            var getHistoryReturn = new List<ScxFileGenerationDetail>
            {
                new ScxFileGenerationDetail(),
                new ScxFileGenerationDetail(),
                new ScxFileGenerationDetail()
            };
            historian.Setup(s => s.GetScxFileGenerationHistory(It.IsAny<int>()))
                .Callback<int>((s) =>
                {
                    getHistoryCalls.Add(s);
                    throw new Exception("Don't be that guy!!!");
                })
                .Returns(getHistoryReturn);
            var tc = new ScxGenerationServiceUnitTestClass(dataRepoFactory.Object,
                proprietaryInventoryService.Object,
                fileService.Object)
            {
                DropFolderPath = dropFolder,
                ScxFileGenerationHistorian = historian.Object
            };
            Exception caught = null;

            try
            {
                tc.GetScxFileGenerationHistory(sourceId);
            }
            catch (Exception e)
            {
                caught = e;
            }

            Assert.IsNotNull(caught);
            Assert.AreEqual(1, getHistoryCalls.Count);
        }

        #endregion // #region GetScxFileGenerationHistory
    }
}