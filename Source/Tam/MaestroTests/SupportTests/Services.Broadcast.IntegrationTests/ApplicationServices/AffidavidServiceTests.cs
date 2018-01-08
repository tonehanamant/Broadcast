using ApprovalTests;
using ApprovalTests.Reporters;
using EntityFrameworkMapping.Broadcast;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavidServiceTests
    {
        private readonly IAffidavitService _Sut;
        private readonly IAffidavitRepository _Repo;

        public AffidavidServiceTests()
        {
            _Sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitService>();
            _Repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService()
        {
            using (new TransactionScopeWrapper())
            {
                AffidavitSaveRequest request = new AffidavitSaveRequest();
                request.FileHash = "abc123";
                request.Source = (int) AffidaviteFileSource.Strata;
                request.FileName = "test.file";

                var detail = new AffidavitSaveRequestDetail();
                detail.AirTime = DateTime.Parse("12/29/2018 10:04AM");
                detail.Isci = "ISCI";
                detail.ProgramName = "Programs R Us";
                detail.SpotLength = 30;
                detail.Station = "WNBC";
                request.Details.Add(detail);

                int id = _Sut.SaveAffidavit(request);

                var affidavite = _Repo.GetAffidavit(id);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(affidavit_files), "created_date");
                jsonResolver.Ignore(typeof(affidavit_files), "id");
                jsonResolver.Ignore(typeof(affidavit_file_details), "id");
                jsonResolver.Ignore(typeof(affidavit_file_details), "affidavit_client_scrubs");
                jsonResolver.Ignore(typeof(affidavit_file_details), "affidavit_file_id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                var json = IntegrationTestHelper.ConvertToJson(affidavite, jsonSettings);
                Approvals.Verify(json);

            }
        }
    }
}

