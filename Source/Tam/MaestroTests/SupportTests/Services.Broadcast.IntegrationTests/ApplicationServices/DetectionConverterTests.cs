using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class DetectionConverterTests
    {
        private readonly IDetectionConverter _IDetectionConverter;
        private readonly IDateAdjustmentEngine _IDateAdjustmentEngine;

        public DetectionConverterTests()
        {
            _IDetectionConverter = IntegrationTestApplicationServiceFactory.GetApplicationService<IDetectionConverter>();
            _IDateAdjustmentEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IDateAdjustmentEngine>();
        }

        [Test]
        public void Should_Convert_To_NSI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(1, 30, 0);

            var result = _IDateAdjustmentEngine.ConvertToNSITime(date, airTime);

            Assert.AreEqual(result.Day, 6);
        }

        [Test]
        public void Should_Not_Convert_To_NSI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(3, 30, 0);

            var result = _IDateAdjustmentEngine.ConvertToNSITime(date, airTime);

            Assert.AreEqual(result.Day, 7);
        }

        [Test]
        public void Should_Convert_To_NTI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(2, 30, 0);

            var result = _IDateAdjustmentEngine.ConvertToNTITime(date, airTime);

            Assert.AreEqual(result.Day, 6);
        }

        [Test]
        public void Should_Not_Convert_To_NTI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(3, 30, 0);

            var result = _IDateAdjustmentEngine.ConvertToNTITime(date, airTime);

            Assert.AreEqual(result.Day, 7);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Tests_DetectionConverter()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Load For Various Tests.xlsx", FileMode.Open, FileAccess.Read);
                var fileName = "BVS Load For Various Tests.xlsx";
                var userName = "Tests_BvsConverter";
                string message = string.Empty;

                var detectionFile = _IDetectionConverter.ExtractDetectionData(stream, "hash", userName, fileName, out message, out Dictionary<TrackerFileDetailKey<DetectionFileDetail>, int> line);

                _VerifyBvsFile(detectionFile);
            }
        }

        [Test]
        [TestCase("BVS Load For Invalid Media Week left limit.xlsx", "Error in row 11: Invalid Media Week for Time Aired")]
        [TestCase("BVS Load For Invalid Media Week right limit.xlsx", "Error in row 12: Invalid Media Week for Time Aired")]
        public void Tests_DetectionConverter_InvalidMediaWeek_LeftLimit(string fileName, string expectedMessage)
        {
            const string userName = "Tests_BvsConverter";

            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read);
                string message = string.Empty;

                var exception = Assert.Throws<ExtractDetectionException>(() => _IDetectionConverter.ExtractDetectionData(stream, "hash", userName, fileName, out message, out Dictionary<TrackerFileDetailKey<DetectionFileDetail>, int> line));
                Assert.AreEqual(expectedMessage, exception.Message);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Tests_LineNumbers_Correct()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Load For Various TestsNoDuplicates.xlsx", FileMode.Open, FileAccess.Read);
                var fileName = "BVS Load For Various Tests.xlsx";
                var userName = "Tests_BvsConverter";
                string message = string.Empty;

                var bvsFile = _IDetectionConverter.ExtractDetectionData(stream, "hash", userName, fileName, out message, out Dictionary<TrackerFileDetailKey<DetectionFileDetail>, int> line);
                int counter = 8;
                foreach (var detail in bvsFile.FileDetails)
                {
                    counter++;
                    Assert.That(line[new TrackerFileDetailKey<DetectionFileDetail>(detail)], Is.EqualTo(counter));
                }

                Assert.That(line.Count == bvsFile.FileDetails.Count);
            }
        }

        private static void _VerifyBvsFile(TrackerFile<DetectionFileDetail> bvsFileInfo)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(TrackerFile<DetectionFileDetail>), "CreatedDate");
            jsonResolver.Ignore(typeof(TrackerFile<DetectionFileDetail>), "Id");
            jsonResolver.Ignore(typeof(DetectionFileDetail), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(bvsFileInfo, jsonSettings);
            Approvals.Verify(json);
        }        
    }

}
