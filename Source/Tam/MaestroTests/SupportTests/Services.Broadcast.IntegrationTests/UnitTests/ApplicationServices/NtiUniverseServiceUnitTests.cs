﻿using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Nti;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class NtiUniverseServiceUnitTests
    {
        private readonly Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private readonly Mock<IBroadcastAudiencesCache> _BroadcastAudiencesCacheMock;
        private readonly Mock<IUniversesFileImporter> _UniversesFileImporterMock;
        private readonly Mock<INtiUniverseRepository> _NtiUniverseRepositoryMock;

        private readonly string _UserName = "UnitTestsUser";
        private readonly DateTime _Now = new DateTime(2019, 7, 1);

        public NtiUniverseServiceUnitTests()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _BroadcastAudiencesCacheMock = new Mock<IBroadcastAudiencesCache>();
            _UniversesFileImporterMock = new Mock<IUniversesFileImporter>();
            _NtiUniverseRepositoryMock = new Mock<INtiUniverseRepository>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<INtiUniverseRepository>())
                .Returns(_NtiUniverseRepositoryMock.Object);
        }

        [Test]
        public void ThrowsException_WhenLoadsUniversesFile_ForMoreThanOneYear()
        {
            const string expectedMessage = "All records must belong to the same year";

            // Arrange
            _UniversesFileImporterMock
                .Setup(x => x.ReadUniverses(It.IsAny<Stream>()))
                .Returns(new List<NtiUniverseExcelRecord>
                {
                    new NtiUniverseExcelRecord { Year = 2019 },
                    new NtiUniverseExcelRecord { Year = 2020 }
                });

            var service = new NtiUniverseService(
                _DataRepositoryFactoryMock.Object,
                _BroadcastAudiencesCacheMock.Object,
                _UniversesFileImporterMock.Object);
            
            // Act
            var caught = Assert.Throws<ApplicationException>(() => service.LoadUniverses(fileStream: It.IsAny<Stream>(), _UserName, _Now));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_WhenLoadsUniversesFile_ForMoreThanOneMonth()
        {
            const string expectedMessage = "All records must belong to the same month";

            // Arrange
            _UniversesFileImporterMock
                .Setup(x => x.ReadUniverses(It.IsAny<Stream>()))
                .Returns(new List<NtiUniverseExcelRecord>
                {
                    new NtiUniverseExcelRecord { Month = 5 },
                    new NtiUniverseExcelRecord { Month = 6 }
                });

            var service = new NtiUniverseService(
                _DataRepositoryFactoryMock.Object,
                _BroadcastAudiencesCacheMock.Object,
                _UniversesFileImporterMock.Object);

            // Act
            var caught = Assert.Throws<ApplicationException>(() => service.LoadUniverses(fileStream: It.IsAny<Stream>(), _UserName, _Now));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_WhenLoadsUniversesFile_ForMoreThanOneWeek()
        {
            const string expectedMessage = "All records must belong to the same week";

            // Arrange
            _UniversesFileImporterMock
                .Setup(x => x.ReadUniverses(It.IsAny<Stream>()))
                .Returns(new List<NtiUniverseExcelRecord>
                {
                    new NtiUniverseExcelRecord { WeekNumber = 1 },
                    new NtiUniverseExcelRecord { WeekNumber = 2 }
                });

            var service = new NtiUniverseService(
                _DataRepositoryFactoryMock.Object,
                _BroadcastAudiencesCacheMock.Object,
                _UniversesFileImporterMock.Object);

            // Act
            var caught = Assert.Throws<ApplicationException>(() => service.LoadUniverses(fileStream: It.IsAny<Stream>(), _UserName, _Now));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_WhenLoadsUniversesFile_WithEmptyAudienceCode()
        {
            const string expectedMessage = "Audience code can not be empty";

            // Arrange
            _UniversesFileImporterMock
                .Setup(x => x.ReadUniverses(It.IsAny<Stream>()))
                .Returns(new List<NtiUniverseExcelRecord>
                {
                    new NtiUniverseExcelRecord { NtiAudienceCode = string.Empty }
                });

            var service = new NtiUniverseService(
                _DataRepositoryFactoryMock.Object,
                _BroadcastAudiencesCacheMock.Object,
                _UniversesFileImporterMock.Object);

            // Act
            var caught = Assert.Throws<ApplicationException>(() => service.LoadUniverses(fileStream: It.IsAny<Stream>(), _UserName, _Now));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_WhenLoadsUniversesFile_WithMissingAudiences()
        {
            const string expectedMessage = "Please provide universes for next audiences: F18-20, F21-24";

            // Arrange
            _UniversesFileImporterMock
                .Setup(x => x.ReadUniverses(It.IsAny<Stream>()))
                .Returns(new List<NtiUniverseExcelRecord>
                {
                    new NtiUniverseExcelRecord { NtiAudienceCode = "HH" }
                });

            _NtiUniverseRepositoryMock
                .Setup(x => x.GetNtiUniverseAudienceMappings())
                .Returns(new List<NtiUniverseAudienceMapping>
                {
                    new NtiUniverseAudienceMapping { NtiAudienceCode = "HH" },
                    new NtiUniverseAudienceMapping { NtiAudienceCode = "F18-20" },
                    new NtiUniverseAudienceMapping { NtiAudienceCode = "F21-24" }
                });

            var service = new NtiUniverseService(
                _DataRepositoryFactoryMock.Object,
                _BroadcastAudiencesCacheMock.Object,
                _UniversesFileImporterMock.Object);

            // Act
            var caught = Assert.Throws<ApplicationException>(() => service.LoadUniverses(fileStream: It.IsAny<Stream>(), _UserName, _Now));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }
    }
}
