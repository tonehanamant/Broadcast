using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Linq;

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
                Station = "NBC",
                LeadInShowType = "News",
                ProgramShowType = "News",
                LeadOutShowType = "News"
            };
        }

        [Test]
        public void ValidateAffidavitRecordValidTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            var result = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);

            Assert.IsTrue(!result.Any());
        }

        [Test]
        public void ValidateAffidavitRecordInvalidProgramNameTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.ProgramName = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "ProgramName" );

            Assert.AreEqual("ProgramName", result.InvalidField);
            Assert.AreEqual("'ProgramName' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidGenreTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Genre = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "Genre");

            Assert.AreEqual("Genre", result.InvalidField);
            Assert.AreEqual("'Genre' is required", result.ErrorMessage);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ValidateAffidavitRecordInvalidLeadInProgramNameTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadInProgramName = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "LeadInProgramName");

            Assert.AreEqual("LeadInProgramName", result.InvalidField);
            Assert.AreEqual("'LeadInProgramName' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidLeadInGenreTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadInGenre = null;

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "LeadInGenre");

            Assert.AreEqual("LeadInGenre", result.InvalidField);
            Assert.AreEqual("'LeadInGenre' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidLeadOutProgramNameTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadOutProgramName = null;

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "LeadOutProgramName");

            Assert.AreEqual("LeadOutProgramName", result.InvalidField);
            Assert.AreEqual("'LeadOutProgramName' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidLeadOutGenreTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadOutGenre = null;

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "LeadOutGenre");

            Assert.AreEqual("LeadOutGenre", result.InvalidField);
            Assert.AreEqual("'LeadOutGenre' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidAirTimeTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.AirTime = DateTime.MinValue;

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "AirTime");

            Assert.AreEqual("AirTime", result.InvalidField);
            Assert.AreEqual("'AirTime' must be a valid date", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidInventorySourceTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.InventorySource = (int)(InventorySourceEnum.Blank);

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "InventorySource");

            Assert.AreEqual("InventorySource", result.InvalidField);
            Assert.AreEqual("'InventorySource' must be valid", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidStationTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Station = null;

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "Station");

            Assert.AreEqual("Station", result.InvalidField);
            Assert.AreEqual("'Station' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidSpotLengthTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.SpotLength = 100;

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "SpotLength");

            Assert.AreEqual("SpotLength", result.InvalidField);
            Assert.AreEqual("'SpotLength' must be valid broadcast spot length: 15,30,60,120,180,300", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidEmptyIsciTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Isci = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "Isci");

            Assert.AreEqual("Isci", result.InvalidField);
            Assert.AreEqual("'Isci' is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidProgramShowTypeTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.ProgramShowType = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "ProgramShowType");

            Assert.AreEqual("ProgramShowType", result.InvalidField);
            Assert.AreEqual("'ProgramShowType' is required",result.ErrorMessage);
        }
        [Test]
        public void ValidateAffidavitRecordInvalidLeadInShowTypeTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadInShowType = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "LeadInShowType");

            Assert.AreEqual("LeadInShowType", result.InvalidField);
            Assert.AreEqual("'LeadInShowType' is required", result.ErrorMessage);
        }
        [Test]
        public void ValidateAffidavitRecordInvalidLeadOutShowTypeTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.LeadOutShowType = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "LeadOutShowType");

            Assert.AreEqual("LeadOutShowType", result.InvalidField);
            Assert.AreEqual("'LeadOutShowType' is required", result.ErrorMessage);
        }
        
        [Test]
        public void ValidateAffidavitRecordInvalidAffiliateTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Affiliate = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "Affiliate");

            Assert.AreEqual("Affiliate", result.InvalidField);
            Assert.AreEqual("'Affiliate' is required",result.ErrorMessage);
        }
    }
}
