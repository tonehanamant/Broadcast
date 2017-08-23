using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
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
                var newDaypartId = sut.OnlyForTests_SaveDaypartInternal(newDaypart);

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
                var existingDaypartId = sut.OnlyForTests_SaveDaypartInternal(existingDaypart);

                Assert.AreEqual(3, existingDaypartId);
            }
        }
    }
}