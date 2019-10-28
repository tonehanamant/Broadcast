using Common.Services;
using NUnit.Framework;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests
{
    [TestFixture]
    public class DaypartCacheTests
    {
        [Test]
        public void TestMultithreadSafety()
        {
            var daypartRepo = new MockDisplayDaypartRepo();
            daypartRepo.CallCount = 0;
            var daypartCache = new DaypartCache(daypartRepo);

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

            public int OnlyForTests_SaveDaypartInternal(DisplayDaypart pDaypart)
            {
                throw new NotImplementedException();
            }

            public int SaveDaypart(DisplayDaypart pDaypart)
            {
                CallCount++;
                return CallCount;
            }
        }
    }
}
