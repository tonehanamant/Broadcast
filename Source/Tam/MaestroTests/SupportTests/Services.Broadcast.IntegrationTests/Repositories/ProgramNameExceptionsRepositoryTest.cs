using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Repositories;
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