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
    public class SigmaConverterTests
    {
        private readonly ISigmaConverter _ISigmaConverter;
        private readonly IDateAdjustmentEngine _IDateAdjustmentEngine;

        public SigmaConverterTests()
        {
            _ISigmaConverter = IntegrationTestApplicationServiceFactory.GetApplicationService<ISigmaConverter>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [ExpectedException(typeof(ExtractBvsException), ExpectedMessage = "Required field IDENTIFIER 1 is null or empty", MatchType = MessageMatch.Contains)]
        public void SigmaConverter_SigmaFile_RequiredFieldEmpty()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\SigmaImportRequiredFieldEmpty.csv", FileMode.Open, FileAccess.Read);
                var fileName = "SigmaImportRequiredFieldEmpty.csv";
                var userName = "BvsConverter_SigmaFile";

                var sigmaFile = _ISigmaConverter.ExtractSigmaData(stream, "hash", userName, fileName, out Dictionary<TrackerFileDetailKey<BvsFileDetail>, int> line);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [ExpectedException(typeof(ExtractBvsException), ExpectedMessage = "Could not find required column IDENTIFIER 1.", MatchType = MessageMatch.Contains)]
        public void SigmaConverter_SigmaFile_RequiredColumnMissing()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\SigmaImportRequiredColumnMissing.csv", FileMode.Open, FileAccess.Read);
                var fileName = "SigmaImportRequiredColumnMissing.csv";
                var userName = "BvsConverter_SigmaFile";
                string message = string.Empty;

                var sigmaFile = _ISigmaConverter.ExtractSigmaData(stream, "hash", userName, fileName, out Dictionary<TrackerFileDetailKey<BvsFileDetail>, int> line);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SigmaConverter_SigmaFile()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\SigmaImport.csv", FileMode.Open, FileAccess.Read);
                var fileName = "SigmaImport.csv";
                var userName = "BvsConverter_SigmaFile";

                var sigmaFile = _ISigmaConverter.ExtractSigmaData(stream, "hash", userName, fileName, out Dictionary<TrackerFileDetailKey<BvsFileDetail>, int> line);

                _VerifySigmaFile(sigmaFile);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [ExpectedException(typeof(ExtractBvsException), ExpectedMessage = "Required field IDENTIFIER 1 is null or empty", MatchType = MessageMatch.Contains)]
        public void SigmaConverter_ExtendedSigmaFile_RequiredFieldsNullOrEmpty()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\ExtendedSigmaImport_RequiredFieldIsNullOrEmpty.csv", FileMode.Open, FileAccess.Read);
                var fileName = "ExtendedSigmaImport_RequiredFieldIsNullOrEmpty.csv";
                var userName = "SigmaConverter";

                var sigmaFile = _ISigmaConverter.ExtractSigmaDataExtended(stream, "hash", userName, fileName, out Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int> line);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [ExpectedException(typeof(ExtractBvsException), ExpectedMessage = "Could not find required column ISCI/AD-ID", MatchType = MessageMatch.Contains)]
        public void SigmaConverter_ExtendedSigmaFile_MissingRequiredField()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\ExtendedSigmaImport_MissingRequiredField.csv", FileMode.Open, FileAccess.Read);
                var fileName = "ExtendedSigmaImport_MissingRequiredField.csv";
                var userName = "SigmaConverter";

                var sigmaFile = _ISigmaConverter.ExtractSigmaDataExtended(stream, "hash", userName, fileName, out Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int> line);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SigmaConverter_ExtendedSigmaFile_ValidFile()
        {
            using (new TransactionScopeWrapper())
            {
                var stream = new FileStream(@".\Files\ExtendedSigmaImport_ValidFile.csv", FileMode.Open, FileAccess.Read);
                var fileName = "ExtendedSigmaImport_ValidFile.csv";
                var userName = "SigmaConverter";

                var sigmaFile = _ISigmaConverter.ExtractSigmaDataExtended(stream, "hash", userName, fileName, out Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int> line);

                _VerifySigmaFile(sigmaFile);
            }
        }

        private static void _VerifySigmaFile<T>(TrackerFile<T> bvsFileInfo) where T : TrackerFileDetail
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(TrackerFile<T>), "CreatedDate");
            jsonResolver.Ignore(typeof(TrackerFile<T>), "Id");
            jsonResolver.Ignore(typeof(T), "Id");

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
