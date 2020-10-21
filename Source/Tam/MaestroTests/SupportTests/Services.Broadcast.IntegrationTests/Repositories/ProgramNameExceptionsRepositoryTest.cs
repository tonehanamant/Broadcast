using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
	public class ProgramNameExceptionsRepositoryTest
	{
		private readonly IProgramNameExceptionsRepository _ProgramNameExceptionsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProgramNameExceptionsRepository>();
		[Test]
		[UseReporter(typeof(DiffReporter))]
		public void GetProgramExceptionsTest()
		{
			List<ProgramNameExceptionDto> programExceptions;

			using (new TransactionScopeWrapper())
			{
				programExceptions = _ProgramNameExceptionsRepository.GetProgramExceptions();
			}

			Approvals.Verify(IntegrationTestHelper.ConvertToJson(programExceptions));
		}

	}
}