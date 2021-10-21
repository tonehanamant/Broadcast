using Common.Services;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Broadcast.Helpers;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Unity;

namespace Services.Broadcast.IntegrationTests
{
    [TestFixture]
    [Category("short_running")]
    public class DaypartCacheTests
    {
        [Test]
        public void TestMultithreadSafety()
        {
            var daypartRepo = new MockDisplayDaypartRepo();
            daypartRepo.CallCount = 0;

            var featureToggleHelper = IntegrationTestApplicationServiceFactory.Instance.Resolve<IFeatureToggleHelper>();
            var configHelper = IntegrationTestApplicationServiceFactory.Instance.Resolve<IConfigurationSettingsHelper>();
            var daypartCache = new DaypartCache(daypartRepo, featureToggleHelper, configHelper);

            var daypartList = new List<DisplayDaypart>();
            for(int i = 0; i < 10; i++)
            {
                daypartList.Add(new DisplayDaypart()
                {
                    Id = 1
                });
            }
            Parallel.ForEach(daypartList, (daypart) =>
            {
                daypartCache.GetIdByDaypart(daypart);
            });
            Assert.AreEqual(1, daypartRepo.CallCount);

        }

        class MockDisplayDaypartRepo : IDisplayDaypartRepository
        {
            public int CallCount { get; set; }

            public List<DaypartCleanupDto> GetAllDaypartsIncludeDays()
            {
                throw new NotImplementedException();
            }

            public DisplayDaypart GetDisplayDaypart(int pDaypartId)
            {
                throw new NotImplementedException();
            }

            public int GetDisplayDaypartIdByText(string pDaypartText)
            {
                throw new NotImplementedException();
            }

            public Dictionary<int, DisplayDaypart> GetDisplayDayparts(IEnumerable<int> pDaypartIds)
            {
                throw new NotImplementedException();
            }

            public int SaveDaypartInternal(DisplayDaypart pDaypart)
            {
                throw new NotImplementedException();
            }

            public int SaveDaypart(DisplayDaypart pDaypart)
            {
                CallCount++;
                return CallCount;
            }

            public DaypartCleanupDto UpdateDaysForDayparts(DaypartCleanupDto daypartCleanupDto)
            {
                throw new NotImplementedException();
            }
        }
    }
}
