using System.Collections.Generic;
using System.Linq;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Repositories;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
	public class ProgramServiceUnitTests
	{
		[Test]
		public void GetProgramsForMappingTest()
		{
			// Arrange
			var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
			var programNameRepository = new Mock<IProgramNameRepository>();
			var genreCacheMock = new Mock<IGenreCache>();
			broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IProgramNameRepository>())
				.Returns(programNameRepository.Object);
			programNameRepository.Setup(p => p.FindProgramFromMapping(It.IsAny<string>()))
				.Returns(new List<ProgramNameMappingDto>
					{new ProgramNameMappingDto {GenreId = 33, ShowTypeId = 1, OfficialProgramName = "Golf"}});
			programNameRepository.Setup(p => p.FindProgramFromExceptions(It.IsAny<string>()))
				.Returns(new List<ProgramNameExceptionDto>
					{new ProgramNameExceptionDto {GenreId = 33, ShowTypeId = 1, CustomProgramName = "Golf 123"}});

			genreCacheMock
				.Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
				.Returns(new LookupDto
				{
					Id = 1,
					Display = "Genre"
				});
			var searchRequest = new SearchRequestProgramDto
			{
				ProgramName = "Golf"
			};

			var service = new ProgramService(genreCacheMock.Object, broadcastDataRepositoryFactory.Object, null, null);

			// Act
			var result = service.GetPrograms(searchRequest, "shubhra");

			// Assert
			Assert.AreEqual(2, result.Count, "Valid Count");
			Assert.AreEqual("Golf", result.FirstOrDefault().Name, "Valid Result");
			Assert.AreNotEqual("NBC", result.FirstOrDefault().Name, "Invalid Result");
		}

		[Test]
		public void GetProgramsForMappingIgnoreProgramFromMapping()
		{
			// Arrange
			var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
			var programNameRepository = new Mock<IProgramNameRepository>();
			var genreCacheMock = new Mock<IGenreCache>();
			broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IProgramNameRepository>())
				.Returns(programNameRepository.Object);
			programNameRepository.Setup(p => p.FindProgramFromMapping(It.IsAny<string>()))
				.Returns(new List<ProgramNameMappingDto>
					{new ProgramNameMappingDto {GenreId = 33, ShowTypeId = 1, OfficialProgramName = "Golf"}});
			programNameRepository.Setup(p => p.FindProgramFromExceptions(It.IsAny<string>()))
				.Returns(new List<ProgramNameExceptionDto>
					{new ProgramNameExceptionDto {GenreId = 33, ShowTypeId = 1, CustomProgramName = "Golf 123"}});

			genreCacheMock
				.Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
				.Returns(new LookupDto
				{
					Id = 1,
					Display = "Genre"
				});
			var searchRequest = new SearchRequestProgramDto
			{
				ProgramName = "Golf",
				IgnorePrograms = {"Golf"}
			};

            var service = new ProgramService(genreCacheMock.Object, broadcastDataRepositoryFactory.Object, null, null);

			// Act
			var result = service.GetPrograms(searchRequest, "shubhra");

			// Assert
			Assert.AreEqual(1, result.Count, "Valid Count");
			Assert.AreEqual("Golf 123", result.FirstOrDefault().Name, "Valid Result");			
		}

		[Test]
		public void GetProgramsForMappingIgnoreProgramFromException()
		{
			// Arrange
			var broadcastDataRepositoryFactory = new Mock<IDataRepositoryFactory>();
			var programNameRepository = new Mock<IProgramNameRepository>();
			var genreCacheMock = new Mock<IGenreCache>();
			broadcastDataRepositoryFactory.Setup(s => s.GetDataRepository<IProgramNameRepository>())
				.Returns(programNameRepository.Object);
			programNameRepository.Setup(p => p.FindProgramFromMapping(It.IsAny<string>()))
				.Returns(new List<ProgramNameMappingDto>
					{new ProgramNameMappingDto {GenreId = 33, ShowTypeId = 1, OfficialProgramName = "Golf"}});
			programNameRepository.Setup(p => p.FindProgramFromExceptions(It.IsAny<string>()))
				.Returns(new List<ProgramNameExceptionDto>
					{new ProgramNameExceptionDto {GenreId = 33, ShowTypeId = 1, CustomProgramName = "Golf 123"}});

			genreCacheMock
				.Setup(x => x.GetGenreLookupDtoById(It.IsAny<int>()))
				.Returns(new LookupDto
				{
					Id = 1,
					Display = "Genre"
				});
			var searchRequest = new SearchRequestProgramDto
			{
				ProgramName = "Golf",
				IgnorePrograms = { "Golf 123" }
			};

            var service = new ProgramService(genreCacheMock.Object, broadcastDataRepositoryFactory.Object, null, null);

			// Act
			var result = service.GetPrograms(searchRequest, "shubhra");

			// Assert
			Assert.AreEqual(1, result.Count, "Valid Count");
			Assert.AreEqual("Golf", result.FirstOrDefault().Name, "Valid Result");
		}
	}
}