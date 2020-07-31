using ApprovalUtilities.Utilities;
using NUnit.Framework;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    public class DaypartHelperUnitTests
    {
        [Test]
        [TestCase(1, 2, 3, 4, 5, 6, 7)]
        [TestCase(1, 2, 3, 4, 5, 6)]
        [TestCase(1, 2, 3, 4, 5)]
        [TestCase(1, 2, 3, 4)]
        [TestCase(1, 2, 3)]
        [TestCase(1, 2)]
        [TestCase(1)]
        [TestCase(0, 8)]
        public void ConvertDaypartDaysToDaypart_IEnumerable(params int[] days)
        {
            var result = DaypartHelper.ConvertDaypartDaysToDaypart(days.ToList());

            Assert.AreEqual(days.Contains(1), result.Monday);
            Assert.AreEqual(days.Contains(2), result.Tuesday);
            Assert.AreEqual(days.Contains(3), result.Wednesday);
            Assert.AreEqual(days.Contains(4), result.Thursday);
            Assert.AreEqual(days.Contains(5), result.Friday);
            Assert.AreEqual(days.Contains(6), result.Saturday);
            Assert.AreEqual(days.Contains(7), result.Sunday);
        }

        [Test]
        [TestCase(1, 2, 3, 4, 5, 6, 7)]
        [TestCase(1, 2, 3, 4, 5, 6)]
        [TestCase(1, 2, 3, 4, 5)]
        [TestCase(1, 2, 3, 4)]
        [TestCase(1, 2, 3)]
        [TestCase(1, 2)]
        [TestCase(1)]
        [TestCase(0, 8)]
        public void ConvertDaypartDaysToDaypart_HashSet(params int[] days)
        {
            var hashSet = new HashSet<int>();
            days.ForEach(d => hashSet.Add(d));
            
            var result = DaypartHelper.ConvertDaypartDaysToDaypart(hashSet);

            Assert.AreEqual(days.Contains(1), result.Monday);
            Assert.AreEqual(days.Contains(2), result.Tuesday);
            Assert.AreEqual(days.Contains(3), result.Wednesday);
            Assert.AreEqual(days.Contains(4), result.Thursday);
            Assert.AreEqual(days.Contains(5), result.Friday);
            Assert.AreEqual(days.Contains(6), result.Saturday);
            Assert.AreEqual(days.Contains(7), result.Sunday);
        }
    }
}
