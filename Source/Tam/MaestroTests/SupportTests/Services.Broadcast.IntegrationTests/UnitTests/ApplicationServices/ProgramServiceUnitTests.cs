using System.Collections.Generic;
using System.Linq;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
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
				.Setup(x => x.GetGenreById(It.IsAny<int>(), It.IsAny<ProgramSourceEnum>()))
				.Returns(new LookupDto
				{
					Id = 1,
					Display = "Genre"
				});
			var searchRequest = new SearchRequestProgramDto
			{
				ProgramName = "Golf"
			};

			var service = new ProgramServiceUnitTestClass(genreCacheMock.Object, null, null,
				broadcastDataRepositoryFactory.Object);
			service.UT_EnableInternalProgramSearch = true;

			// Act
			var result = service.GetPrograms(searchRequest, "shubhra");

			// Assert
			Assert.AreEqual(2, result.Count, "Valid Count");
			Assert.AreEqual("Golf", result.FirstOrDefault().Name, "Valid Result");
			Assert.AreNotEqual("NBC", result.FirstOrDefault().Name, "Invalid Result");
		}
	}
}