using System.Transactions;
using EntityFrameworkMapping.Broadcast;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Services.Broadcast.BusinessEngines;
using Tam.Maestro.Common.DataLayer;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class PostExcelReportGeneratorTests
    {
        private readonly IRatingForecastService _RatingForecastService = IntegrationTestApplicationServiceFactory.GetApplicationService<IRatingForecastService>();

        [Test]
        [Ignore]
        public void GenerateReportFile()
        {
            using (new TransactionScopeWrapper())
            {
                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest.xlsx";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.Live;
                files.posting_book_id = 410;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                var postUploadFileDetails = PostEngineIntegrationTests.GetParsedDetail();
                postUploadFileDetails.post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 33, impression = 1234 });
                postUploadFileDetails.post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 34, impression = 12345 });
                files.post_file_details.Add(postUploadFileDetails);

                var postUploadFileDetails2 = PostEngineIntegrationTests.GetParsedDetail();
                postUploadFileDetails2.post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 33, impression = 1234 });
                postUploadFileDetails2.post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 34, impression = 12345 });
                files.post_file_details.Add(postUploadFileDetails2);


                var x = new PostExcelReportGenerator(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory, IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionAdjustmentEngine>(), _RatingForecastService);
                var report = x.Generate(files.Convert());
                File.WriteAllBytes(report.Filename, report.Stream.GetBuffer());
                Assert.IsNotNull(report.Stream);
            }
        }

        [Test]
        public void Impressions_Have_Correct_Format()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest.xlsx";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.Live;
                files.posting_book_id = 413;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                var postUploadFileDetails = PostEngineIntegrationTests.GetParsedDetail();
                postUploadFileDetails.post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 33, impression = 1234 });
                postUploadFileDetails.post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 34, impression = 12345 });
                files.post_file_details.Add(postUploadFileDetails);

                var x = new PostExcelReportGenerator(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory, IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionAdjustmentEngine>(), _RatingForecastService);
                var report = x.GenerateExcelPackage(files.Convert());
                var column = report.Workbook.Worksheets.First().Dimension.Columns;
                var row = report.Workbook.Worksheets.First().Dimension.Rows;

                for (var i = 2; i <= row; i++)
                {
                    var cell = report.Workbook.Worksheets.First().Cells[i, column];
                    if (cell.Value != null)
                        Assert.IsTrue(cell.Style.Numberformat.Format == "#,#");
                }
            }
        }
    }
}
