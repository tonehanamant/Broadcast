using ApprovalTests;
using ApprovalTests.Reporters;
using EntityFrameworkMapping.Broadcast;
using NUnit.Framework;
using Services.Broadcast.Extensions;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class DetectionExtensionsUnitTests
    {
        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        public void IsInSpec(int status, bool expected)
        {
            var detectionFileDetails = _GetDetectionFileDetails(status);

            var result = detectionFileDetails.IsInSpec();

            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, true)]
        [TestCase(3, true)]
        public void IsOutOfSpec(int status, bool expected)
        {
            var detectionFileDetails = _GetDetectionFileDetails(status);

            var result = detectionFileDetails.IsOutOfSpec();

            Assert.AreEqual(expected, result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void InSpec()
        {
            var list = _GetDetectionFileDetailsList();

            var result = list.InSpec();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OutOfSpec()
        {
            var list = _GetDetectionFileDetailsList();

            var result = list.OutOfSpec();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 3)]
        public void ClearStatus(int status, int expected)
        {
            var detectionFileDetails = _GetDetectionFileDetails(status);

            detectionFileDetails.ClearStatus();

            Assert.AreEqual(expected, detectionFileDetails.status);
        }

        private detection_file_details _GetDetectionFileDetails(int status) =>
            new detection_file_details()
            {
                status = status
            };

        private List<detection_file_details> _GetDetectionFileDetailsList() =>
            new List<detection_file_details>
            {
                _GetDetectionFileDetails(0),
                _GetDetectionFileDetails(1),
                _GetDetectionFileDetails(2),
                _GetDetectionFileDetails(3)
            };
    }
}
