using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Broadcast.IntegrationTests.ApplicationServices;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class PostRepositoryTests
    {
        IPostRepository _Repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Test_GetAllPostedProposals()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _Repo.GetAllPostedProposals();

                var jsonResolver = new IgnorableSerializerContractResolver();
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }
    }
}
