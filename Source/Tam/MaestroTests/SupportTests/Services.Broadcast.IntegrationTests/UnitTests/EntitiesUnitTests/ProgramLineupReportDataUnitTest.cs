using NUnit.Framework;
using Services.Broadcast.Entities.Campaign;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.EntitiesUnitTests
{
    [TestFixture]
    [Category("short_running")]
    public class ProgramLineupReportDataUnitTest
    {
        const int AM12_05 = 299;
        const int AM4 = 14400;
        const int AM10 = 35999;
        const int AM11 = 39600;
        const int PM1 = 46799;
        const int PM4 = 57600;
        const int PM7 = 68399;
        const int PM8 = 72000;
        
        [Test]
        [TestCase(19080, 21570, AM4, AM10, true)] //5:18-5:59:30
        [TestCase(14400, 15000, AM4, AM10, true)]  //4-4:10
        [TestCase(15000, 14500, AM4, AM10, false)]  //4:10-4:01:40
        [TestCase(30000, 40000, AM4, AM10, false)]  //8:20-11:06:40
        [TestCase(36000, 39600, AM4, AM10, false)]  //10-11
        [TestCase(7200, 14500, AM4, AM10, false)] //2-4:01:40
        [TestCase(68400, 72000, AM4, AM10, false)]  //19-20
        public void IsMorningNews(int daypartStartTime, int daypartEndTime
            , int startTime, int endTime, bool expectedResult)
        {
            bool result = ProgramLineupReportData.CheckDaypartForRollup(new DisplayDaypart
            {
                StartTime = daypartStartTime,
                EndTime = daypartEndTime
            }, startTime, endTime);
            Assert.AreEqual(expectedResult, result);
        }

        //[Test]
        [TestCase(46800, 50400, AM11, PM1, false)]  //13-14
        [TestCase(39600, 46799, AM11, PM1, true)]  //11-13
        [TestCase(40000, 45000, AM11, PM1, true)]  //11:06:40-12:30
        [TestCase(39000, 45000, AM11, PM1, false)]  //10:50-12:30
        [TestCase(45000, 47000, AM11, PM1, false)]  //12:30-13:03:20
        [TestCase(45000, 43000, AM11, PM1, false)]  //12:30-11:56:40
        [TestCase(68400, 72000, AM11, PM1, false)]  //19-20
        public void IsMiddayNews(int daypartStartTime, int daypartEndTime
            , int startTime, int endTime, bool expectedResult)
        {
            bool result = ProgramLineupReportData.CheckDaypartForRollup(new DisplayDaypart
            {
                StartTime = daypartStartTime,
                EndTime = daypartEndTime
            }, startTime, endTime);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(64800, 66600, PM4, PM7, true)] // 6PM-6:30PM
        [TestCase(57600, 68399, PM4, PM7, true)] // 4PM-7PM
        [TestCase(54000, 68400, PM4, PM7, false)] // 3PM-7PM
        [TestCase(68400, 72000, PM4, PM7, false)] // 7PM-8PM
        [TestCase(61200, 57600, PM4, PM7, false)] // 5PM-4PM
        public void IsEveningNews(int daypartStartTime, int daypartEndTime
            , int startTime, int endTime, bool expectedResult)
        {
            bool result = ProgramLineupReportData.CheckDaypartForRollup(new DisplayDaypart
            {
                StartTime = daypartStartTime,
                EndTime = daypartEndTime
            }, startTime, endTime);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(75600, 79200, PM8, AM12_05, true)]  //21-22
        [TestCase(79200, 250, PM8, AM12_05, true)]  //22-12:04:10
        [TestCase(72000, 79200, PM8, AM12_05, true)]  //20-22
        [TestCase(100, 299, PM8, AM12_05, true)]   //12:01:40 - 12:04:59
        [TestCase(68400, 72000, PM8, AM12_05, false)]  //19-20
        [TestCase(299, 100, PM8, AM12_05, false)]   //12:04:59 - 12:01:40
        [TestCase(100, 3600, PM8, AM12_05, false)]   //12:01:40 - 01:00
        [TestCase(57600, 68399, PM8, AM12_05, false)] // 4PM-7PM
        [TestCase(82800, 75600, PM8, AM12_05, false)] // 11PM-9PM
        [TestCase(240, 120, PM8, AM12_05, false)] // 12:04 - 12:02
        [TestCase(240, 75600, PM8, AM12_05, false)] // 12:04 - 9PM
        public void IsLateNews(int daypartStartTime, int daypartEndTime
            , int startTime, int endTime, bool expectedResult)
        {
            bool result = ProgramLineupReportData.CheckDaypartForRollup(new DisplayDaypart
            {
                StartTime = daypartStartTime,
                EndTime = daypartEndTime
            }, startTime, endTime);
            Assert.AreEqual(expectedResult, result);
        }
    }
}
