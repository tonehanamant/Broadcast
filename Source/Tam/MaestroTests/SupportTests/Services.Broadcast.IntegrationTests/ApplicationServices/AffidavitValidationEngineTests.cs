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
                InventorySource = AffidaviteFileSourceEnum.Strata,
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
                ShowType = "News",
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
        public void ValidateAffidavitRecordInvalidShowTypeTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.ShowType = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "ShowType");

            Assert.AreEqual("ShowType", result.InvalidField);
            Assert.AreEqual("'ShowType' is required",result.ErrorMessage);
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
