using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class ScheduleRepositoryTests
    {
        private IScheduleRepository _ScheduleRepository =
                        IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IScheduleRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetDisplaySchedules_DateRange_Start_End()
        {
            DateTime? startDate = DateTime.Parse("2016-07-11");
            DateTime? endDate = DateTime.Parse("2016-12-26");
            var response = _ScheduleRepository.GetDisplaySchedules(startDate, endDate);

            Console.WriteLine(response.Count + " Records returned.");
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void GetDisplaySchedules_DateRange_null_Start()
        {
            DateTime? endDate = DateTime.Parse("2016-07-11");
            var response = _ScheduleRepository.GetDisplaySchedules(null, endDate);

            Console.WriteLine(response.Count + " Records returned.");
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        [Ignore("We do not support this case")]
        [UseReporter(typeof(DiffReporter))]
        public void GetDisplaySchedules_DateRange_null_End()
        {
            DateTime? startDate = DateTime.Parse("2016-07-11");
            var response = _ScheduleRepository.GetDisplaySchedules(startDate, null);

            Console.WriteLine(response.Count + " Records returned.");
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        //PRI-8165 We no longer support the ALL option
        public void GetDisplaySchedules_DateRange_null_Start_End()
        {
            var response = _ScheduleRepository.GetDisplaySchedules(null, null);

            Assert.IsTrue(response.Count == 0);
        }
    }
}