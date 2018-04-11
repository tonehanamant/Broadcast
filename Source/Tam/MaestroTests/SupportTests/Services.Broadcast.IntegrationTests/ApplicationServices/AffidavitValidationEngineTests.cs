using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitValidationEngineTests
    {
        private IAffidavitValidationEngine _AffidavitValidationEngine =
    IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitValidationEngine>();

        private AffidavitSaveRequestDetail _SetupAffidavitSaveRequestDetail()
        {
            return new AffidavitSaveRequestDetail()
            {
                Affiliate = "AA",
                AirTime = new DateTime(2018, 01, 01),
                Genre = "Comedy",
                InventorySource = 1,
                Isci = "AAAAAAA",
                LeadInGenre = "Comedy",
                LeadInProgramName = "Saturday Night Live",
                LeadOutGenre = "Comedy",
                LeadOutProgramName = "Saturday Night Live",
                Market = "Boston",
                ProgramName = "Saturday Night Live",
                SpotCost = 10,
                SpotLength = 15,
                Station = "NBC"
            };
        }

        [Test]
        public void ValidateAffidavitRecordValidTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidProgramNameTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.ProgramName = "";

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("ProgramName", result.InvalidField);
            Assert.AreEqual("'ProgramName' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidGenreTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Genre = "";

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Genre", result.InvalidField);
            Assert.AreEqual("'Genre' is required", result.ErrorMessage);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ValidateAffidavitRecordInvalidLeadInProgramNameTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadInProgramName = "";

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("LeadInProgramName", result.InvalidField);
            Assert.AreEqual("'LeadInProgramName' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidLeadInGenreTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadInGenre = null;

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("LeadInGenre", result.InvalidField);
            Assert.AreEqual("'LeadInGenre' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidLeadOutProgramNameTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadOutProgramName = null;

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("LeadOutProgramName", result.InvalidField);
            Assert.AreEqual("'LeadOutProgramName' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidLeadOutGenreTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadOutGenre = null;

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("LeadOutGenre", result.InvalidField);
            Assert.AreEqual("'LeadOutGenre' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidAirTimeTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.AirTime = DateTime.MinValue;

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("AirTime", result.InvalidField);
            Assert.AreEqual("'AirTime' must be a valid date", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidInventorySourceTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.InventorySource = (int)(InventorySourceEnum.Blank);

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("InventorySource", result.InvalidField);
            Assert.AreEqual("'InventorySource' must be valid", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidStationTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Station = null;

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Station", result.InvalidField);
            Assert.AreEqual("'Station' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidSpotLengthTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.SpotLength = 100;

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("SpotLength", result.InvalidField);
            Assert.AreEqual("'SpotLength' must be valid broadcast spot length: 15,30,60,120,180,300", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidEmptyIsciTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Isci = "";

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Isci", result.InvalidField);
            Assert.AreEqual("'Isci' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidAffiliateTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Affiliate = "";

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Affiliate", result.InvalidField);
            Assert.AreEqual("'Affiliate' is required",result.ErrorMessage);
        }
    }
}
