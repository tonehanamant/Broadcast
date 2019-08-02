using NUnit.Framework;
using ApprovalTests.Reporters;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;
using ApprovalTests;
using IntegrationTests.Common;
using Newtonsoft.Json;
using System;

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
