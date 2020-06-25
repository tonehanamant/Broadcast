using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.UnitTests.Converters
{
    [TestFixture]
    public class VpvhFileImporterUnitTests
    {
        private IVpvhFileImporter _VpvhFileImporter;

        [SetUp]
        public void SetUp()
        {
            _VpvhFileImporter = new VpvhFileImporter();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReadVpvhs_Success()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            var file = new FileStream(filename, FileMode.Open, FileAccess.Read);

            var records = _VpvhFileImporter.ReadVpvhs(file);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(records));
        }

        [Test]
        public void ReadVpvhs_Fail()
        {
            Assert.That(() => _VpvhFileImporter.ReadVpvhs(null)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid VPVH file."));
        }
    }
}
