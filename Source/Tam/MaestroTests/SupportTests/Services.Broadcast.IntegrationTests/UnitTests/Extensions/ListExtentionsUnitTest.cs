using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Extensions;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class ListExtentionsUnitTest
    {
        [Test]
        public void IsEmpty_NotEmpty()
        {
            var list = _GetList();

            var result = ListExtentions.IsEmpty(list);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsEmpty_Null()
        {
            var list = _GetList();
            list = null;

            var result = ListExtentions.IsEmpty(list);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsEmpty_Empty()
        {
            var list = _GetList();
            list.Clear();

            var result = ListExtentions.IsEmpty(list);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsAny_ContainsItem()
        {
            var list = _GetList();
            var containsList = new List<LookupDto> { list.First(), _GetLookupDto() };

            var result = ListExtentions.ContainsAny(list, containsList);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsAny_DoesntContainsItem()
        {
            var list = _GetList();
            var containsList = new List<LookupDto> { _GetLookupDto() };

            var result = ListExtentions.ContainsAny(list, containsList);

            Assert.IsFalse(result);
        }

        [Test]
        public void TakeOut()
        {
            var list = _GetList();
            var result = ListExtentions.TakeOut(list, 1);

            Assert.AreEqual(2, result.Id);
            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void DoubleSumOrDefault()
        {
            var list = new List<object> { 15d, 71.7, -19.2, 55.9 };

            var result = ListExtentions.DoubleSumOrDefault(list);

            Assert.AreEqual(123.4, result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetChunks()
        {
            var list = _GetList();

            var result = ListExtentions.GetChunks(list, 2);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupConnectedItems()
        {
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var result = ListExtentions.GroupConnectedItems(list, (n1, n2) => n1 + n2 > 10);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<LookupDto> _GetList()
        {
            return new List<LookupDto>
            {
                 new LookupDto(1, "One"),
                 new LookupDto(2, "Two"),
                 new LookupDto(3,"Three")
            };
        }

        private LookupDto _GetLookupDto() =>
            new LookupDto(4, "Four");
    }
}
