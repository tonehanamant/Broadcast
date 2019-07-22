using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Scx;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ScxFileGeneration
{
    public class ScxFileGenerationHistorianUnitTests
    {
        #region Constructor

        [Test]
        public void ConstructorTest()
        {
            var repo = new Mock<IScxGenerationJobRepository>();
            var calculator = new Mock<IQuarterCalculationEngine>();
            var dropFolder = string.Empty;

            var tc = new ScxFileGenerationHistorian(repo.Object, calculator.Object, dropFolder);

            Assert.IsNotNull(tc);
        }

        #endregion // #region Constructor

        #region GetScxFileGenerationHistory

        [Test]
        public void GetScxFileGenerationHistoryWithNoData()
        {
            var getDetailsReturn = new List<ScxFileGenerationDetailDto>();
            var getDetailsCalls = new List<int>();
            var repo = new Mock<IScxGenerationJobRepository>();
            repo.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((i) => { getDetailsCalls.Add(i); })
                .Returns(getDetailsReturn);
            var calculator = new Mock<IQuarterCalculationEngine>();
            const string dropFolder = "IAmADropFolder";
            const int sourceId = 7;
            var tc = new ScxFileGenerationHistorian(repo.Object, calculator.Object, dropFolder);

            var result = tc.GetScxFileGenerationHistory(sourceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(1, getDetailsCalls.Count);
            Assert.AreEqual(0, getDetailsReturn.Count);
        }

        [Test]
        public void GetScxFileGenerationHistoryWithData()
        {
            var getDetailsReturn = new List<ScxFileGenerationDetailDto>
            {
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw()
            };
            var getDetailsCalls = new List<int>();
            var repo = new Mock<IScxGenerationJobRepository>();
            repo.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((i) => { getDetailsCalls.Add(i); })
                .Returns(getDetailsReturn);
            var calculator = new Mock<IQuarterCalculationEngine>();
            const string dropFolder = "IAmADropFolder";
            const int sourceId = 7;
            var tc = new ScxFileGenerationHistorian(repo.Object, calculator.Object, dropFolder);

            var result = tc.GetScxFileGenerationHistory(sourceId);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, getDetailsCalls.Count);
            Assert.AreEqual(3, getDetailsReturn.Count);
        }

        [Test]
        public void GetScxFileGenerationHistoryWithException()
        {
            var getDetailsReturn = new List<ScxFileGenerationDetailDto>
            {
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw(),
                GetPopulatedDetailRaw()
            };
            var getDetailsCalls = new List<int>();
            var repo = new Mock<IScxGenerationJobRepository>();
            repo.Setup(s => s.GetScxFileGenerationDetails(It.IsAny<int>()))
                .Callback<int>((i) =>
                {
                    getDetailsCalls.Add(i);
                    throw new Exception("You asked for this!!!");
                })
                .Returns(getDetailsReturn);
            var calculator = new Mock<IQuarterCalculationEngine>();
            var dropFolder = "IAmADropFolder";
            var sourceId = 7;
            var tc = new ScxFileGenerationHistorian(repo.Object, calculator.Object, dropFolder);
            Exception caught = null;

            try
            {
                tc.GetScxFileGenerationHistory(sourceId);
            }
            catch (Exception e)
            {
                caught = e;
            }

            Assert.AreEqual(1, getDetailsCalls.Count);
            Assert.IsNotNull(caught);
            Assert.IsTrue(caught.Message.Contains("You asked for this!!!"));
        }

        #endregion // #region GetScxFileGenerationHistory

        #region Helpers

        private ScxFileGenerationDetailDto GetPopulatedDetailRaw()
        {
            var detail = new ScxFileGenerationDetailDto
            {
                GenerationRequestDateTime = new DateTime(2017, 10, 17, 19, 30, 3),
                GenerationRequestedByUsername = "SomeGuy",
                FileName = "IFile.txt",
                UnitName = "IUnit",
                DaypartCodeId = 6,
                DaypartCodeName = "NN",
                StartDateTime = new DateTime(2017, 10, 17, 19, 30, 3),
                EndDateTime = new DateTime(2017, 11, 17, 19, 30, 3),
                ProcessingStatusId = 1
            };
            return detail;
        }

        #endregion // #region Helpers
    }
}