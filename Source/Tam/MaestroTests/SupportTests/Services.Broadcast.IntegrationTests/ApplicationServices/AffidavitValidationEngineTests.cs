using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavitValidationEngineTests
    {
        private IAffidavitValidationEngine _AffidavitValidationEngine =
    IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitValidationEngine>();

        private InboundFileSaveRequestDetail _SetupAffidavitSaveRequestDetail()
        {
            return new InboundFileSaveRequestDetail()
            {
                Affiliate = "AA",
                Genre = "Comedy",
                InventorySource = DeliveryFileSourceEnum.Strata,
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
        public void ValidateAffidavitRecordInvalidAirTimeTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.AirTime = DateTime.MinValue;

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "AirTime");

            Assert.AreEqual("AirTime", result.InvalidField);
            Assert.AreEqual("must be a valid date", result.ErrorMessage);
        }


        [Test]
        public void ValidateAffidavitRecordInvalidStationTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Station = null;

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "Station");

            Assert.AreEqual("Station", result.InvalidField);
            Assert.AreEqual("is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidSpotLengthTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.SpotLength = 100;

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "SpotLength");

            Assert.AreEqual("SpotLength", result.InvalidField);
            Assert.AreEqual("must be valid broadcast spot length: 15,30,60,120,180,300", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidEmptyIsciTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Isci = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "Isci");

            Assert.AreEqual("Isci", result.InvalidField);
            Assert.AreEqual("is required", result.ErrorMessage);
        }

        [Test]
        public void ValidateAffidavitRecordInvalidAffiliateTest()
        {
            var affidavitSaveRequestDetail = _SetupAffidavitSaveRequestDetail();

            affidavitSaveRequestDetail.Affiliate = "";

            var results = _AffidavitValidationEngine.ValidateAffidavitRecord(affidavitSaveRequestDetail);
            var result = results.First(r => r.InvalidField == "AirTime");

            Assert.AreEqual("AirTime", result.InvalidField);
            Assert.AreEqual("must be a valid date", result.ErrorMessage);
        }
    }
}
