using ApprovalTests;
using ApprovalTests.Reporters;
using EntityFrameworkMapping.Broadcast;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class BvsConverterTests
    {
        private readonly IBvsConverter _IBvsConverter;

        public BvsConverterTests()
        {
            _IBvsConverter = IntegrationTestApplicationServiceFactory.GetApplicationService<IBvsConverter>();
        }

        [Test]
        public void Should_Convert_To_NSI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(1, 30, 0);

            var result = _IBvsConverter.ConvertToNSITime(date, airTime);

            Assert.AreEqual(result.Day, 6);
        }

        [Test]
        public void Should_Not_Convert_To_NSI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(3, 30, 0);

            var result = _IBvsConverter.ConvertToNSITime(date, airTime);

            Assert.AreEqual(result.Day, 7);
        }
        
        [Test]
        public void Should_Convert_To_NTI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(2, 30, 0);

            var result = _IBvsConverter.ConvertToNTITime(date, airTime);

            Assert.AreEqual(result.Day, 6);
        }
        
        [Test]
        public void Should_Not_Convert_To_NTI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(3, 30, 0);

            var result = _IBvsConverter.ConvertToNTITime(date, airTime);

            Assert.AreEqual(result.Day, 7);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Tests_BvsConverter()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\BVS Load For Various Tests.xlsx", FileMode.Open, FileAccess.Read);
                var fileName = "BVS Load For Various Tests.xlsx";
                var userName = "Tests_BvsConverter";
                string message = string.Empty;

                var bvsFile = _IBvsConverter.ExtractBvsData(stream, "hash", userName, fileName, out message, out Dictionary<BvsFileDetailKey, int> line);
                
                _VerifyBvsFile(bvsFile);
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

                var bvsFile = _IBvsConverter.ExtractBvsData(stream, "hash", userName, fileName, out message, out Dictionary<BvsFileDetailKey, int> line);
                int counter = 8;
                foreach (var detail in bvsFile.BvsFileDetails)
                {
                    counter++;
                    Assert.That(line[new BvsFileDetailKey(detail)], Is.EqualTo(counter));
                }

                Assert.That(line.Count == bvsFile.BvsFileDetails.Count);
            }
        }

        private static void _VerifyBvsFile(BvsFile bvsFileInfo)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(BvsFile), "CreatedDate");
            jsonResolver.Ignore(typeof(BvsFile), "Id");
            jsonResolver.Ignore(typeof(BvsFileDetail), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var json = IntegrationTestHelper.ConvertToJson(bvsFileInfo, jsonSettings);
            Approvals.Verify(json);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [ExpectedException(typeof(ExtractBvsException), ExpectedMessage = "Required field IDENTIFIER 1 is null or empty", MatchType = MessageMatch.Contains)]
        public void BvsConverter_SigmaFile_RequiredFieldEmpty()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\SigmaImportRequiredFieldEmpty.csv", FileMode.Open, FileAccess.Read);
                var fileName = "SigmaImportRequiredFieldEmpty.csv";
                var userName = "BvsConverter_SigmaFile";
                string message = string.Empty;

                var sigmaFile = _IBvsConverter.ExtractSigmaData(stream, "hash", userName, fileName, out Dictionary<BvsFileDetailKey, int> line);

                _VerifyBvsFile(sigmaFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [ExpectedException(typeof(ExtractBvsException), ExpectedMessage = "Could not find required column IDENTIFIER 1.", MatchType = MessageMatch.Contains)]
        public void BvsConverter_SigmaFile_RequiredColumnMissing()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\SigmaImportRequiredColumnMissing.csv", FileMode.Open, FileAccess.Read);
                var fileName = "SigmaImportRequiredColumnMissing.csv";
                var userName = "BvsConverter_SigmaFile";
                string message = string.Empty;

                var sigmaFile = _IBvsConverter.ExtractSigmaData(stream, "hash", userName, fileName, out Dictionary<BvsFileDetailKey, int> line);

                _VerifyBvsFile(sigmaFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void BvsConverter_SigmaFile()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\SigmaImport.csv", FileMode.Open, FileAccess.Read);
                var fileName = "SigmaImport.csv";
                var userName = "BvsConverter_SigmaFile";
                string message = string.Empty;

                var sigmaFile = _IBvsConverter.ExtractSigmaData(stream, "hash", userName, fileName, out Dictionary<BvsFileDetailKey, int> line);

                _VerifyBvsFile(sigmaFile);
            }
        }
    }
}
