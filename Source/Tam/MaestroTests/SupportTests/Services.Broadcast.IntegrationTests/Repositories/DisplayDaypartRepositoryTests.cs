using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]  
    [Category("short_running")]
    public class DisplayDaypartRepositoryTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveDaypartInternal_NewDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDisplayDaypartRepository>();

                var newDaypart = DisplayDaypart.ParseMsaDaypart("M-W 5:45PM-6:33 PM");
                var newDaypartId = sut.SaveDaypartInternal(newDaypart);

                var daypart = sut.GetDisplayDaypart(newDaypartId);

                var settings = IntegrationTestHelper._GetJsonSettings();
                ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(DisplayDaypart), "_Id");
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(daypart, settings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveDaypartInternal_ExistingDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDisplayDaypartRepository>();

                var existingDaypart = DisplayDaypart.ParseMsaDaypart("M-SU 6PM-12AM");
                var existingDaypartId = sut.SaveDaypartInternal(existingDaypart);

                Assert.AreEqual(3, existingDaypartId);
            }
        }

        [Test]
        [TestCase("M-SU 4AM-2:05AM")]
        [TestCase("M-SU 6AM-2:05AM")]
        [TestCase("M-SU 6AM-2:05AM")]
        [TestCase("M-SU 6AM-2:05AM")]
        [TestCase("M-SU 6AM-9AM")]
        [TestCase("M-SU 2AM-6AM")]
        [TestCase("M-SU 9AM-4PM")]
        [TestCase("M-SU 6AM-2:05AM")]
        [TestCase("M-SU 11PM-2AM")]
        [TestCase("M-SU 8PM-11PM")]
        [TestCase("M-SU 6PM-8PM")]
        [TestCase("M-SU 3PM-6PM")]
        [TestCase("M-SU 4AM-12:05AM")]
        [TestCase("M-SU 4AM-12:05AM")]
        [TestCase("M-SU 4PM-12:05AM")]
        [TestCase("M-SU 8PM-12:05AM")]
        [TestCase("M-SU 1PM-12:05AM")]
        [TestCase("M-SU 4PM-7PM")]
        [TestCase("M-SU 4AM-1PM")]
        [TestCase("M-SU 11AM-1PM")]
        [TestCase("M-SU 4AM-10AM")]
        public void SaveDaypartInternal_DefaultDaypartsAreNotDuplicated(string daypartText)
        {
            /*
             * PRI-21850
             * The above are the daypart_default dayparts used for Planning.
             * We want to ensure that we didn't duplicate records in the dayparts table.
             */
            using (new TransactionScopeWrapper())
            {
                var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDisplayDaypartRepository>();

                var testDaypart = DisplayDaypart.ParseMsaDaypart(daypartText);
                Assert.DoesNotThrow(() => sut.SaveDaypartInternal(testDaypart));
            }
        }
    }
}