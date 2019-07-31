using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Scx;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.IntegrationTests.UnitTests.ScxFileGeneration
{
    public class ScxFileGenerationDetailTransformerTests
    {
        #region TransformFromDtoToEntity

        [Test]
        public void TransformFromDtoToEntityOfRawHappyPath()
        {
            const string dropFolder = "IAmADropFolder";
            const int processingStatusRaw = 1;
            var startDate = new DateTime();
            var endDate = new DateTime();
            var dto = GetBase(startDate, endDate, processingStatusRaw);
            var getAllQuartersBetweenDatesResults = new List<QuarterDetailDto>
            {
                new QuarterDetailDto {Quarter = 1, Year = 2019}
            };
            var getAllQuartersBetweenDatesCalls = new List<Tuple<DateTime, DateTime>>();
            var calculator = new Mock<IQuarterCalculationEngine>();
            calculator.Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((s, e) => getAllQuartersBetweenDatesCalls.Add(new Tuple<DateTime, DateTime>(s, e)))
                .Returns(getAllQuartersBetweenDatesResults);

            var result = ScxFileGenerationDetailTransformer.TransformFromDtoToEntity(dto, calculator.Object, dropFolder);

            Assert.IsNotNull(result);
            AssertBaseTransform(dto, result);
            AssertCalculatedFields(result, BackgroundJobProcessingStatus.Queued, getAllQuartersBetweenDatesResults.Count);
            Assert.AreEqual(1, getAllQuartersBetweenDatesCalls.Count);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item1, startDate);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item2, endDate);
        }

        [Test]
        public void FinalizeTransformationOfRawWithMultipleQuarters()
        {
            const string dropFolder = "IAmADropFolder";
            const int processingStatusRaw = 1;
            var startDate = new DateTime();
            var endDate = new DateTime();
            var dto = GetBase(startDate, endDate, processingStatusRaw);
            var getAllQuartersBetweenDatesResults = new List<QuarterDetailDto>
            {
                new QuarterDetailDto {Quarter = 1, Year = 2019},
                new QuarterDetailDto {Quarter = 2, Year = 2019},
                new QuarterDetailDto {Quarter = 3, Year = 2019},
                new QuarterDetailDto {Quarter = 4, Year = 2019},
                new QuarterDetailDto {Quarter = 1, Year = 2018},
                new QuarterDetailDto {Quarter = 2, Year = 2018},
                new QuarterDetailDto {Quarter = 3, Year = 2018},
                new QuarterDetailDto {Quarter = 4, Year = 2018},
                new QuarterDetailDto {Quarter = 4, Year = 2017}
            };
            var getAllQuartersBetweenDatesCalls = new List<Tuple<DateTime, DateTime>>();
            var calculator = new Mock<IQuarterCalculationEngine>();
            calculator.Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((s, e) => getAllQuartersBetweenDatesCalls.Add(new Tuple<DateTime, DateTime>(s, e)))
                .Returns(getAllQuartersBetweenDatesResults);

            var result = ScxFileGenerationDetailTransformer.TransformFromDtoToEntity(dto, calculator.Object, dropFolder);

            Assert.IsNotNull(result);
            AssertBaseTransform(dto, result);
            AssertCalculatedFields(result, BackgroundJobProcessingStatus.Queued, getAllQuartersBetweenDatesResults.Count);
            Assert.AreEqual(1, getAllQuartersBetweenDatesCalls.Count);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item1, startDate);
            Assert.AreEqual(getAllQuartersBetweenDatesCalls[0].Item2, endDate);
        }

        #endregion // #region TransformFromDtoToEntity

        #region Helpers

        private ScxFileGenerationDetailDto GetBase(DateTime startDate, DateTime endDate, int processingStatusId)
        {
            var raw = new ScxFileGenerationDetailDto
            {
                GenerationRequestDateTime = new DateTime(2019, 10, 17, 12, 23, 33),
                GenerationRequestedByUsername = "TestUser",
                FileId = 12,
                UnitName = "U1",
                DaypartCode = "EMN",
                ProcessingStatusId = processingStatusId,
                StartDateTime = startDate,
                EndDateTime = endDate
            };
            return raw;
        }

        private void AssertBaseTransform(ScxFileGenerationDetailDto dto, ScxFileGenerationDetail entity)
        {
            Assert.AreEqual(dto.GenerationRequestDateTime, entity.GenerationRequestDateTime);
            Assert.AreEqual(dto.GenerationRequestedByUsername, entity.GenerationRequestedByUsername);
            Assert.AreEqual(dto.UnitName, entity.UnitName);
            Assert.AreEqual(dto.FileId, entity.FileId);
            Assert.AreEqual(dto.DaypartCode, entity.DaypartCode);
        }

        private void AssertCalculatedFields(ScxFileGenerationDetail entity, BackgroundJobProcessingStatus expectedStatus, int expectedQuartersCount)
        {
            Assert.AreEqual(expectedStatus, entity.ProcessingStatus);
            Assert.AreEqual(expectedQuartersCount, entity.QuarterDetails.Count);
        }

        #endregion // #region Helpers
    }
}