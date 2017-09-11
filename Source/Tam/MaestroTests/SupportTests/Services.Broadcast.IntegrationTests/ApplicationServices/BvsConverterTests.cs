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
        private readonly IBvsConverter _Sut;

        public BvsConverterTests()
        {
            _Sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IBvsConverter>();
        }

        [Test]
        public void Should_Convert_To_NSI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(1, 30, 0);

            var result = _Sut.ConvertToNSITime(date, airTime);

            Assert.AreEqual(result.Day, 6);
        }

        [Test]
        public void Should_Not_Convert_To_NSI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(3, 30, 0);

            var result = _Sut.ConvertToNSITime(date, airTime);

            Assert.AreEqual(result.Day, 7);
        }


        [Test]
        public void Should_Convert_To_NTI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(2, 30, 0);

            var result = _Sut.ConvertToNTITime(date, airTime);

            Assert.AreEqual(result.Day, 6);
        }


        [Test]
        public void Should_Not_Convert_To_NTI_Time()
        {
            var date = new DateTime(2016, 11, 7);
            var airTime = new TimeSpan(3, 30, 0);

            var result = _Sut.ConvertToNTITime(date, airTime);

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

                Dictionary<BvsFileDetailKey, int> line;
                var bvsFile = _Sut.ExtractBvsData(stream, "hash", userName, fileName, out message, out line);
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

                Dictionary<BvsFileDetailKey, int> line;
                var bvsFile = _Sut.ExtractBvsData(stream, "hash", userName, fileName, out message, out line);
                int counter = 8;
                foreach (var detail in bvsFile.bvs_file_details)
                {
                    counter++;
                    Assert.That(line[new BvsFileDetailKey(detail)], Is.EqualTo(counter));
                }

                Assert.That(line.Count == bvsFile.bvs_file_details.Count);

            }
        }

        private static void _VerifyBvsFile(bvs_files bvsFileInfo)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(bvs_files), "created_date");
            jsonResolver.Ignore(typeof(bvs_files), "id");

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
