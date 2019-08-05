using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PostingTypesServiceIntegrationTests
    {
        private readonly IPostingTypeService _PostingTypeService;

        public PostingTypesServiceIntegrationTests()
        {
            _PostingTypeService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostingTypeService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAll()
        {
            using (new TransactionScopeWrapper())
            {
                var spotLengths = _PostingTypeService.GetPostingTypes();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(spotLengths));
            }
        }        
    }
}
