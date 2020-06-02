using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities.Vpvh;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    public class VpvhServiceUnitTests
    {
        private const string IntegrationTestUser = "IntegrationTestUser";
        private readonly DateTime CreatedDate = new DateTime(2022, 5, 14);

        private IVpvhService _VpvhService;
        private Mock<IBroadcastAudiencesCache> _BroadcastAudiencesCacheMock;
        private Mock<IVpvhFileImporter> _VpvhFileImporterMock;
        private Mock<IVpvhRepository> _VpvhRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _BroadcastAudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
            _VpvhFileImporterMock = new Mock<IVpvhFileImporter>();
            _VpvhRepositoryMock = new Mock<IVpvhRepository>();

            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<IVpvhRepository>())
                .Returns(_VpvhRepositoryMock.Object);

            _VpvhService = new VpvhService(dataRepositoryFactoryMock.Object, _VpvhFileImporterMock.Object, _BroadcastAudiencesCacheMock.Object);
        }

        [Test]
        public void LoadVpvhs_FileAlreadyUploaded()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";
            _VpvhRepositoryMock.Setup(v => v.HasFile(It.IsAny<string>())).Returns(true);

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("VPVH file already uploaded to the system."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Never);
        }

        [Test]
        public void LoadVpvhs_InvalidFile()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Throws(new Exception("Invalid VPVH file."));

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid VPVH file."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
        }

        [Test]
        public void LoadVpvhs_InvalidAudience()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { _GetRecord() });

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid audience."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
        }

        [Test]
        public void LoadVpvhs_InvalidQuarter()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            var record = _GetRecord();
            record.Quarter = "5Q17";
            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { record });
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDisplayAudienceByCode(It.IsAny<string>())).Returns(new DisplayAudience { Id = 31 });

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid quarter."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
        }

        [Test]
        public void LoadVpvhs_MaxVpvh()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            var record = _GetRecord();
            record.SynAll = 11;
            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { record });
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDisplayAudienceByCode(It.IsAny<string>())).Returns(new DisplayAudience { Id = 31 });

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("VPVH must be between 0.01 and 10."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
        }

        [Test]
        public void LoadVpvhs_MinVpvh()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            var record = _GetRecord();
            record.SynAll = -11;
            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { record });
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDisplayAudienceByCode(It.IsAny<string>())).Returns(new DisplayAudience { Id = 31 });

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("VPVH must be between 0.01 and 10."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
        }

        [Test]
        public void LoadVpvhs_InvalidAMNews()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            var record = _GetRecord();
            record.AMNews = -11;
            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { record });
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDisplayAudienceByCode(It.IsAny<string>())).Returns(new DisplayAudience { Id = 31 });

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("VPVH must be between 0.01 and 10."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
        }

        [Test]
        public void LoadVpvhs_InvalidPMNews()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            var record = _GetRecord();
            record.PMNews = -11;
            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { record });
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDisplayAudienceByCode(It.IsAny<string>())).Returns(new DisplayAudience { Id = 31 });

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("VPVH must be between 0.01 and 10."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
        }

        [Test]
        public void LoadVpvhs_InvalidSynAll()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            var record = _GetRecord();
            record.SynAll = -11;
            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { record });
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDisplayAudienceByCode(It.IsAny<string>())).Returns(new DisplayAudience { Id = 31 });

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("VPVH must be between 0.01 and 10."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
        }

        [Test]
        public void LoadVpvhs_DuplicateRow()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { _GetRecord(), _GetRecord() });
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDisplayAudienceByCode(It.IsAny<string>())).Returns(new DisplayAudience { Id = 31 });

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Duplicate values to W18+ audience."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadVpvhs_Success()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { _GetRecord() });
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDisplayAudienceByCode(It.IsAny<string>())).Returns(new DisplayAudience { Id = 31, AudienceString = "Woman 18+" });

            VpvhFile vpvhFile = null;
            _VpvhRepositoryMock.Setup(r => r.SaveFile(It.IsAny<VpvhFile>())).Callback<VpvhFile>(c => vpvhFile = c);
            _VpvhRepositoryMock.Setup(r => r.GetVpvhMappings()).Returns(new List<VpvhAudienceMapping>());

            _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate);

            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(vpvhFile));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void LoadVpvhs_FailObjectSaved()
        {
            const string filename = @".\Files\Vpvh\VPVH_valid.xlsx";

            _VpvhFileImporterMock.Setup(v => v.ReadVpvhs(It.IsAny<Stream>())).Returns(new List<VpvhExcelRecord> { _GetRecord(), _GetRecord() });
            _BroadcastAudiencesCacheMock.Setup(a => a.GetDisplayAudienceByCode(It.IsAny<string>())).Returns(new DisplayAudience { Id = 31 });

            VpvhFile vpvhFile = null;
            _VpvhRepositoryMock.Setup(r => r.SaveFile(It.IsAny<VpvhFile>())).Callback<VpvhFile>(c => vpvhFile = c);

            Assert.That(() => _VpvhService.LoadVpvhs(new FileStream(filename, FileMode.Open, FileAccess.Read), "VPVH_valid.xlsx", IntegrationTestUser, CreatedDate)
                , Throws.TypeOf<Exception>().With.Message.EqualTo("Duplicate values to W18+ audience."));
            _VpvhRepositoryMock.Verify(r => r.SaveFile(It.IsAny<VpvhFile>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(vpvhFile));
        }


        private VpvhExcelRecord _GetRecord() =>
            new VpvhExcelRecord
            {
                AMNews = 1,
                Audience = "W18+",
                PMNews = 1,
                Quarter = "1Q17",
                SynAll = 1
            };
    }
}
