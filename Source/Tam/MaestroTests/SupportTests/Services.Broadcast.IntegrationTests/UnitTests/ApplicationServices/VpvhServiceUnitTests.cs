using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
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
        private Mock<IVpvhExportEngine> _VpvhExportEngine;
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IDaypartDefaultRepository> _DaypartDefaultRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _BroadcastAudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
            _VpvhFileImporterMock = new Mock<IVpvhFileImporter>();
            _VpvhRepositoryMock = new Mock<IVpvhRepository>();
            _VpvhExportEngine = new Mock<IVpvhExportEngine>();
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _DaypartDefaultRepositoryMock = new Mock<IDaypartDefaultRepository>();

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(CreatedDate);

            dataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IVpvhRepository>())
                .Returns(_VpvhRepositoryMock.Object);

            dataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IDaypartDefaultRepository>())
                .Returns(_DaypartDefaultRepositoryMock.Object);

            _VpvhRepositoryMock
                .Setup(x => x.GetQuartersWithVpvhData())
                .Returns(_GetQuartersWithVpvhData());

            _DaypartDefaultRepositoryMock
                .Setup(x => x.GetAllDaypartDefaults())
                .Returns(_GetDaypartDefaults());

            _VpvhRepositoryMock
                .Setup(x => x.GetQuartersByYears(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetVpvhQuarters());

            _VpvhService = new VpvhService(
                dataRepositoryFactoryMock.Object, 
                _VpvhFileImporterMock.Object, 
                _BroadcastAudiencesCacheMock.Object, 
                _VpvhExportEngine.Object,
                _DateTimeEngineMock.Object);
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetVpvhDefaults()
        {
            // Arrange
            var request = new VpvhDefaultsRequest
            {
                AudienceIds = new List<int> { 1, 31 }
            };

            var passedYears = new List<IEnumerable<int>>();
            _VpvhRepositoryMock
                .Setup(x => x.GetQuartersByYears(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetVpvhQuarters())
                .Callback<IEnumerable<int>>(x => passedYears.Add(x));
                
            // Act
            var vpvhDefaults = _VpvhService.GetVpvhDefaults(request);

            // Assert
            var result = new
            {
                vpvhDefaults,
                passedYears
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void CanNotGetVpvhDefaults_WhenThereIsNotEnoughVPVHData()
        {
            // Arrange
            var request = new VpvhDefaultsRequest
            {
                AudienceIds = new List<int> { 1 }
            };

            _VpvhRepositoryMock
                .Setup(x => x.GetQuartersWithVpvhData())
                .Returns(_GetQuartersWithVpvhData().Take(3).ToList());

            // Act
            var exception = Assert.Throws<Exception>(() => _VpvhService.GetVpvhDefaults(request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("There must VPVH data for at least 4 quarters"));
        }

        [Test]
        public void CanNotGetVpvhDefaults_WhenThereIsNoVPVHDataForQuarter()
        {
            // Arrange
            var request = new VpvhDefaultsRequest
            {
                AudienceIds = new List<int> { 1 }
            };

            _VpvhRepositoryMock
                .Setup(x => x.GetQuartersByYears(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetVpvhQuarters().Where(x => !(x.Quarter == 1 && x.Year == 2019)).ToList());

            // Act
            var exception = Assert.Throws<Exception>(() => _VpvhService.GetVpvhDefaults(request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("There is no VPVH data. Audience id: 1, quarter: Q1 2019"));
        }

        [Test]
        public void CanNotGetVpvhDefaults_WhenThereAreMoreThanOneVPVHRecordForTheSameQuarterAndAudienceFound()
        {
            // Arrange
            var request = new VpvhDefaultsRequest
            {
                AudienceIds = new List<int> { 1 }
            };

            _VpvhRepositoryMock
                .Setup(x => x.GetQuartersByYears(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetVpvhQuarters().Union(new List<VpvhQuarter> 
                {
                    new VpvhQuarter
                    {
                        Audience = new DisplayAudience { Id = 1 },
                        Quarter = 1,
                        Year = 2019,
                        AMNews = 0.1,
                        PMNews = 0.2,
                        SynAll = 0.3,
                        Tdn = 0.4,
                        Tdns = 0.5
                    }
                }).ToList());

            // Act
            var exception = Assert.Throws<Exception>(() => _VpvhService.GetVpvhDefaults(request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("More than one VPVH record exists. Audience id: 1, quarter: Q1 2019"));
        }

        [Test]
        public void CanNotGetVpvhDefaults_WhenUnknownVpvhCalculationSourceTypeEnumWasDiscovered()
        {
            // Arrange
            var request = new VpvhDefaultsRequest
            {
                AudienceIds = new List<int> { 1 }
            };

            var allDaypartDefaults = _GetDaypartDefaults();
            allDaypartDefaults.First().VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)999999;

            _DaypartDefaultRepositoryMock
                .Setup(x => x.GetAllDaypartDefaults())
                .Returns(allDaypartDefaults);

            // Act
            var exception = Assert.Throws<Exception>(() => _VpvhService.GetVpvhDefaults(request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Unknown VpvhCalculationSourceTypeEnum was discovered"));
        }

        private List<DaypartDefaultDto> _GetDaypartDefaults()
        {
            return new List<DaypartDefaultDto>
            {
                new DaypartDefaultDto { Id = 1, Code = "EMN", FullName = "Early Morning News", VpvhCalculationSourceType = VpvhCalculationSourceTypeEnum.AM_NEWS },
                new DaypartDefaultDto { Id = 3, Code = "EN", FullName = "Evening News", VpvhCalculationSourceType = VpvhCalculationSourceTypeEnum.PM_NEWS },
                new DaypartDefaultDto { Id = 7, Code = "PA", FullName = "Prime Access", VpvhCalculationSourceType = VpvhCalculationSourceTypeEnum.SYN_All },
                new DaypartDefaultDto { Id = 17, Code = "TDN", FullName = "Total Day News", VpvhCalculationSourceType = VpvhCalculationSourceTypeEnum.AVG_OF_AM_NEWS_AND_PM_NEWS },
                new DaypartDefaultDto { Id = 22, Code = "TDNS", FullName = "Total Day News and Syndication", VpvhCalculationSourceType = VpvhCalculationSourceTypeEnum.AVG_OF_AM_NEWS_AND_PM_NEWS_AND_SYN_ALL },
            };
        }

        private List<QuarterDto> _GetQuartersWithVpvhData()
        {
            return new List<QuarterDto>
            {
                new QuarterDto { Year = 2019, Quarter = 1 },
                new QuarterDto { Year = 2021, Quarter = 1 },
                new QuarterDto { Year = 2018, Quarter = 1 },
                new QuarterDto { Year = 2017, Quarter = 1 },
                new QuarterDto { Year = 2018, Quarter = 3 },
            };
        }

        private List<VpvhQuarter> _GetVpvhQuarters()
        {
            return new List<VpvhQuarter>
            {
                new VpvhQuarter
                {
                    Audience = new DisplayAudience { Id = 1 },
                    Quarter = 1,
                    Year = 2019,
                    AMNews = 0.1,
                    PMNews = 0.2,
                    SynAll = 0.3,
                    Tdn = 0.4,
                    Tdns = 0.5
                },
                new VpvhQuarter
                {
                    Audience = new DisplayAudience { Id = 1 },
                    Quarter = 1,
                    Year = 2021,
                    AMNews = 0.1,
                    PMNews = 0.2,
                    SynAll = 0.3,
                    Tdn = 0.4,
                    Tdns = 0.5
                },
                new VpvhQuarter
                {
                    Audience = new DisplayAudience { Id = 1 },
                    Quarter = 1,
                    Year = 2018,
                    AMNews = 0.1,
                    PMNews = 0.2,
                    SynAll = 0.3,
                    Tdn = 0.4,
                    Tdns = 0.5
                },
                new VpvhQuarter
                {
                    Audience = new DisplayAudience { Id = 1 },
                    Quarter = 3,
                    Year = 2018,
                    AMNews = 0.1,
                    PMNews = 0.2,
                    SynAll = 0.3,
                    Tdn = 0.4,
                    Tdns = 0.5
                },
                new VpvhQuarter
                {
                    Audience = new DisplayAudience { Id = 1 },
                    Quarter = 2,
                    Year = 2018,
                    AMNews = 0.1,
                    PMNews = 0.2,
                    SynAll = 0.3,
                    Tdn = 0.4,
                    Tdns = 0.5
                }
            };
        }
    }
}
