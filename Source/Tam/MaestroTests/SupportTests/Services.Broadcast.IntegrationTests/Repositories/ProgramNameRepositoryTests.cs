using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
	[TestFixture]
	public class ProgramNameRepositoryTests
	{
		[Test]
		public void FindProgramsExceptionsTest()
		{
			string expectedProgramName = "golf";
			var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
				.GetDataRepository<IProgramNameRepository>();
			using (new TransactionScopeWrapper())
			{
				var result = repo.FindProgramFromExceptions(expectedProgramName);
				var actual = result.Any(p=>p.CustomProgramName.ToLower().Equals(expectedProgramName));
				Assert.IsTrue(actual);
			}
		}
	}
}

