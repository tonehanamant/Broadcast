using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using log4net.Core;
using Microsoft.EntityFrameworkCore.Design;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Utilities;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.Post;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostPrePostingService : IApplicationService
    {
        int SavePost(PostRequest request);
        List<PostPrePostingFile> GetPosts();
        PostPrePostingFile GetPost(int uploadId);
        PostPrePostingFileSettings GetPostSettings(int uploadId);
        bool DeletePost(int id);
        ReportOutput GenerateReportWithImpression(int id);

        ReportOutput GenerateReportWithImpression_V2(int id);
        ReportOutput GenerateReportWithImpression_V3(int id);

        int EditPost(PostRequest request);
        PostPrePostingDto GetInitialData();
    }

    public class PostPrePostingService : BroadcastBaseClass, IPostPrePostingService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IPostEngine _PostEngine;
        private readonly IPostFileParserFactory _PostFileParserFactory;
        private readonly IReportGenerator<PostPrePostingFile> _PostReportGenerator;
        private readonly IRatingForecastService _RatingForecastService;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        internal static readonly string InvalidPlaybackTypeErrorMessage = "PlaybackType {0} is not valid.";
        internal static readonly string FileNotExcelErroMessage = "File does not end with '.xlsx'.";
        internal static readonly string NoAudiencesErrorMessage = "Must have at least one Audience.";
        internal static readonly string InvalidAudienceErrorMessage = "Invalid Audience Id provided.";
        internal static readonly string MissingId = "File missing Id.";
        internal static readonly string DuplicateFileErrorMessage = "Could not import file, it has been imported. Delete existing file if needed";

        public PostPrePostingService(IDataRepositoryFactory broadcastDataRepositoryFactory, IPostEngine postEngine, IPostFileParserFactory postFileParserFactoryFactory, IReportGenerator<PostPrePostingFile> postReportGenerator, IRatingForecastService ratingForecastService, IBroadcastAudiencesCache audiencesCache)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _PostEngine = postEngine;
            _PostFileParserFactory = postFileParserFactoryFactory;
            _PostReportGenerator = postReportGenerator;
            _RatingForecastService = ratingForecastService;
            _AudiencesCache = audiencesCache;
        }

        public int SavePost(PostRequest request)
        {
            const string TIMER_TOTAL_DURATION = "Total Duration";
            const string TIMER_STEP_VALIDATE_REQUEST = "Validate Request";
            const string TIMER_STEP_PARSE_FILE = "Parse File";
            const string TIMER_STEP_SAVE_POST = "Save Post";
            const string TIMER_STEP_POST = "Post";

            _LogInfo($"SavePost beginning for file '{request.FileName}'");

            var timers = new ProcessWorkflowTimers();
            timers.Start(TIMER_TOTAL_DURATION);
            
            try
            {
                if (!request.FileName.EndsWith(".xlsx"))
                    throw new ApplicationException(FileNotExcelErroMessage);

                timers.Start(TIMER_STEP_VALIDATE_REQUEST);
                ValidateRequest(request);
                timers.End(TIMER_STEP_VALIDATE_REQUEST);

                using (var excelPackage = new ExcelPackage(request.PostStream))
                {
                    timers.Start(TIMER_STEP_PARSE_FILE);
                    var postFileParser = _PostFileParserFactory.CreateParser(excelPackage);

                    var postFileDetails = postFileParser.ParseExcel(excelPackage);
                    timers.End(TIMER_STEP_PARSE_FILE);

                    var postFile = new post_files
                    {
                        equivalized = request.Equivalized,
                        posting_book_id = request.PostingBookId,
                        playback_type = (byte)request.PlaybackType,
                        file_name = request.FileName,
                        upload_date = DateTime.Now,
                        modified_date = DateTime.Now,
                        post_file_details = postFileDetails,
                        post_file_demos = request.Audiences.Select(a => new post_file_demos { demo = a }).ToList()
                    };

                    timers.Start(TIMER_STEP_SAVE_POST);
                    var id = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().SavePost(postFile);
                    timers.End(TIMER_STEP_SAVE_POST);

                    timers.Start(TIMER_STEP_POST);
                    _PostEngine.Post(postFile);
                    timers.End(TIMER_STEP_POST);

                    return id;
                }
            }
            catch (Exception ex)
            {
                _LogError($"Exception caught processing the file : {ex.Message}", ex);
                throw ex;
            }
            finally
            {
                timers.End(TIMER_TOTAL_DURATION);
                var timersReport = timers.ToString();
                _LogInfo($"SavePost commpleted for file '{request.FileName}'.  Timers Report : '{timersReport}'");
            }
        }

        public int EditPost(PostRequest request)
        {
            if (!request.FileId.HasValue)
                throw new ApplicationException(MissingId);

            ValidateRequest(request);

            _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().DeletePostImpressions(request.FileId.Value);

            var file = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().GetPostEF(request.FileId.Value);
            //Check that this removes the old demos then add new ones instead of just adding demos
            file.post_file_demos = request.Audiences.Select(a => new post_file_demos { demo = a }).ToList();
            file.equivalized = request.Equivalized;
            file.playback_type = (byte)request.PlaybackType;
            file.posting_book_id = request.PostingBookId;
            file.modified_date = DateTime.Now;

            var id = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().SavePost(file);

            _PostEngine.Post(file);

            return id;
        }

        private void ValidateRequest(PostRequest request)
        {
            if (request.Audiences == null || !request.Audiences.Any())
                throw new ApplicationException(NoAudiencesErrorMessage);

            var validAudiences = _AudiencesCache.GetAllLookups().Select(a => a.Id).ToList();
            foreach(var audience in request.Audiences)
            {
                if (!validAudiences.Contains(audience))
                {
                    _LogError($"Invalid audience Id: {audience} found while validating post file {request.FileName}.");
                    throw new ApplicationException(InvalidAudienceErrorMessage);
                }
            }


            if (request.PlaybackType != ProposalEnums.ProposalPlaybackType.Live &&
                request.PlaybackType != ProposalEnums.ProposalPlaybackType.LivePlus1 &&
                request.PlaybackType != ProposalEnums.ProposalPlaybackType.LivePlus3 &&
                request.PlaybackType != ProposalEnums.ProposalPlaybackType.LivePlus7 &&
                request.PlaybackType != ProposalEnums.ProposalPlaybackType.LiveSameDay)
                throw new ApplicationException(string.Format(InvalidPlaybackTypeErrorMessage, (char)request.PlaybackType));

            if (_BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().PostExist(request.FileName))
                throw new ApplicationException(string.Format(DuplicateFileErrorMessage, request.FileName));
        }

        public PostPrePostingDto GetInitialData()
        {
            return new PostPrePostingDto
            {
                PostingBooks = _RatingForecastService.GetPostingBooks(),
                PlaybackTypes = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalPlaybackType>(),
                Demos = _AudiencesCache.GetAllLookups()
            };
        }

        public ReportOutput GenerateReportWithImpression(int id)
        {
            const string TIMER_TOTAL_DURATION = "Total Duration";
            const string TIMER_STEP_GET_POST = "Get Post";
            const string TIMER_STEP_GENERATE = "Generate";

            _LogInfo($"GenerateReportWithImpression beginning for file id '{id}'");

            var timers = new ProcessWorkflowTimers();
            timers.Start(TIMER_TOTAL_DURATION);

            long totalBytesOfMemoryUsed = -1;

            try
            {
                timers.Start(TIMER_STEP_GET_POST);
                var post = GetPost(id);
                timers.End(TIMER_STEP_GET_POST);

                var currentProcess = Process.GetCurrentProcess();
                totalBytesOfMemoryUsed = currentProcess.WorkingSet64;
                
                _LogInfo($"GenerateReportWithImpression retreived the posting data for '{id}'.  Timers Report : '{timers.ToString()}'; Bytes : {totalBytesOfMemoryUsed};");

                timers.Start(TIMER_STEP_GENERATE);
                var result = _PostReportGenerator.Generate(post);
                timers.End(TIMER_STEP_GENERATE);

                return result;
            }
            catch (Exception ex)
            {
                timers.End(TIMER_TOTAL_DURATION);
                var timersReport = timers.ToString();
                _LogError($"GenerateReportWithImpression errored for file id '{id}'.  Timers Report : '{timersReport}'; Bytes : {totalBytesOfMemoryUsed};", ex);
                return null;
            }
            finally
            {
                timers.End(TIMER_TOTAL_DURATION);
                var timersReport = timers.ToString();
                _LogInfo($"GenerateReportWithImpression completed for file id '{id}'.  Timers Report : '{timersReport}'; Bytes : {totalBytesOfMemoryUsed};");
            }
        }

        public ReportOutput GenerateReportWithImpression_V2(int postFileId)
        {
            const string TIMER_TOTAL_DURATION = "Total Duration";
            const string TIMER_STEP_GET_IMPRESSIONS = "Get Impressions";
            const string TIMER_STEP_SETUP = "Setup";
            const string TIMER_STEP_GET_POST = "Get Post";
            const string TIMER_STEP_GENERATE = "Generate";
            
            var detailImpressionsChunkSize = 1000;

            _LogInfo($"GenerateReportWithImpression_V2 beginning for file id '{postFileId}'");
            var timers = new ProcessWorkflowTimers();
            timers.Start(TIMER_TOTAL_DURATION);
            timers.Start(TIMER_STEP_SETUP);


            var repo = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>();

            var settings = repo.GetPostSettings(postFileId);
            var demos = repo.GetPostDemos(postFileId);
            var allDemos = _AudiencesCache.GetAllLookups();
            var demoLookups = allDemos.Where(a => demos.Contains(a.Id)).ToList();

            var postFile = new PostPrePostingFile
            {
                Id = postFileId,
                FileName = settings.FileName,
                Equivalized = settings.Equivalized,
                PostingBookId = settings.PostingBookId,
                PlaybackType = settings.PlaybackType,
                UploadDate = settings.UploadDate,
                ModifiedDate = settings.ModifiedDate,
                DemoLookups = demoLookups,
                Demos = demos,
                FileDetails = new List<PostFileDetail>()
            };

            timers.End(TIMER_STEP_SETUP);

            _LogInfo($"GenerateReportWithImpression_V2 Setup complete for '{postFileId}'; Timers : {timers}");
            
            try
            {
                timers.Start(TIMER_STEP_GET_POST);
                var details = repo.GetPostDetailsId(postFileId);
                timers.End(TIMER_STEP_GET_POST);

                timers.Start(TIMER_STEP_GET_IMPRESSIONS);
                var chunks = details.GetChunks(detailImpressionsChunkSize);
                _LogInfo($"GenerateReportWithImpression_V2 Setup complete for '{postFileId}'; Timers : {timers}");

                var chunkIndex = 0;
                var totalChunks = chunks.Count;

                foreach (var chunk in chunks)
                {
                    chunkIndex++;
                    var detailIds = chunk.Select(s => s).ToList();
                    var lineImpressions = repo.GetPostFileDetailImpressions(detailIds);
                    postFile.FileDetails.AddRange(lineImpressions);
                    _LogInfo($"GenerateReportWithImpression_V2 Calculation chunk {chunkIndex} of {totalChunks} for '{postFileId}'; FileDetails Count : {postFile.FileDetails.Count}");
                }

                timers.End(TIMER_STEP_GET_IMPRESSIONS);
                _LogInfo($"GenerateReportWithImpression_V2 Info gathering iteration completeed for '{postFileId}'. Timers : {timers}");

                timers.Start(TIMER_STEP_GENERATE);
                var result = _PostReportGenerator.Generate(postFile);
                timers.End(TIMER_STEP_GENERATE);

                _LogInfo($"GenerateReportWithImpression_V2 File generation complete for '{postFileId}'; Timers : {timers}");

                return result;
            }
            catch (Exception ex)
            {
                timers.End(TIMER_TOTAL_DURATION);
                var timersReport = timers.ToString();
                _LogError($"GenerateReportWithImpression errored for file id '{postFileId}'.  Timers Report : '{timersReport}';", ex);
                return null;
            }
            finally
            {
                timers.End(TIMER_TOTAL_DURATION);
                var timersReport = timers.ToString();
                _LogInfo($"GenerateReportWithImpression completed for file id '{postFileId}'.  Timers Report : '{timersReport}';");
            }
        }

        public ReportOutput GenerateReportWithImpression_V3(int postFileId)
        {
            const string TIMER_TOTAL_DURATION = "Total Duration";
            const string TIMER_STEP_FINALIZE = "Finalize";
            const string TIMER_STEP_SETUP = "Setup";
            const string TIMER_STEP_GET_POST = "Get Post";
            const string TIMER_STEP_GENERATE = "Generate";

            var detailImpressionsChunkSize = 5000;

            _LogInfo($"GenerateReportWithImpression_V3 beginning for file id '{postFileId}'");
            var timers = new ProcessWorkflowTimers();
            timers.Start(TIMER_TOTAL_DURATION);
            timers.Start(TIMER_STEP_SETUP);

            var repo = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>();

            var settings = repo.GetPostSettings(postFileId);
            var demos = repo.GetPostDemos(postFileId);
            var allDemos = _AudiencesCache.GetAllLookups();
            var demoLookups = allDemos.Where(a => demos.Contains(a.Id)).ToList();

            var postFile = new PostPrePostingFile
            {
                Id = postFileId,
                FileName = settings.FileName,
                Equivalized = settings.Equivalized,
                PostingBookId = settings.PostingBookId,
                PlaybackType = settings.PlaybackType,
                UploadDate = settings.UploadDate,
                ModifiedDate = settings.ModifiedDate,
                DemoLookups = demoLookups,
                Demos = demos,
                FileDetails = new List<PostFileDetail>()
            };

            var excelWrapper = ((PostExcelReportGenerator)_PostReportGenerator).GetExcelPackageBase(postFile);

            timers.End(TIMER_STEP_SETUP);

            _LogInfo($"GenerateReportWithImpression_V3 Setup complete for '{postFileId}'; Timers : {timers}");

            try
            {
                timers.Start(TIMER_STEP_GET_POST);
                var details = repo.GetPostDetailsId(postFileId);
                timers.End(TIMER_STEP_GET_POST);

                timers.Start(TIMER_STEP_GENERATE);
                var chunks = details.GetChunks(detailImpressionsChunkSize);
                _LogInfo($"GenerateReportWithImpression_V3 got {details.Count} detail ids for '{postFileId}'; Timers : {timers}");

                var chunkIndex = 0;
                var totalChunks = chunks.Count;

                foreach (var chunk in chunks)
                {
                    chunkIndex++;
                    var detailIds = chunk.Select(s => s).ToList();
                    var lineImpressions = repo.GetPostFileDetailImpressions(detailIds);

                    ((PostExcelReportGenerator)_PostReportGenerator).AppendDetailsToReport(excelWrapper, lineImpressions);

                    _LogInfo($"GenerateReportWithImpression_V3 Calculation chunk {chunkIndex} of {totalChunks} for '{postFileId}'; FileDetails Count : {excelWrapper.TotalDetailsCount}");
                }

                timers.Start(TIMER_STEP_FINALIZE);
                var result = ((PostExcelReportGenerator)_PostReportGenerator).FinalizeReportOutput(excelWrapper);
                timers.End(TIMER_STEP_FINALIZE);

                var currentProcess = Process.GetCurrentProcess();
                var totalBytesOfMemoryUsed = currentProcess.WorkingSet64;

                timers.End(TIMER_STEP_GENERATE);

                _LogInfo($"GenerateReportWithImpression_V3 File generation complete for '{postFileId}'; FileDetails Count : {excelWrapper.TotalDetailsCount}; TotalBytesOfMemoryUsed : {totalBytesOfMemoryUsed}; Timers : {timers}");

                return result;
            }
            catch (Exception ex)
            {
                timers.End(TIMER_TOTAL_DURATION);
                var timersReport = timers.ToString();
                _LogError($"GenerateReportWithImpression_V3 errored for file id '{postFileId}'.  Timers Report : '{timersReport}';", ex);
                return null;
            }
            finally
            {
                timers.End(TIMER_TOTAL_DURATION);
                var timersReport = timers.ToString();
                _LogInfo($"GenerateReportWithImpression_V3 completed for file id '{postFileId}'.  Timers Report : '{timersReport}';");
            }
        }

        public List<PostPrePostingFile> GetPosts()
        {
            var allDemos = _AudiencesCache.GetAllLookups().ToDictionary(l => l.Id);

            var postFiles = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().GetAllPostsList();

            foreach( var pf in postFiles)
            {
                var demoLookups = new List<LookupDto>();
                foreach(var demo in pf.Demos)
                {
                    var found = allDemos.TryGetValue(demo, out LookupDto lookup);
                    if (found)
                    {
                        demoLookups.Add(lookup);
                    }
                    else
                    {
                        _LogWarning($"Invalid audience Id: {demo} found for file {pf.FileName} while loading posting file list.");
                    }
                }
                pf.DemoLookups = demoLookups;
            }

            return postFiles;
        }

        public PostPrePostingFile GetPost(int uploadId)
        {
            var allDemos = _AudiencesCache.GetAllLookups().ToDictionary(l => l.Id);

            var postFile = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().GetPost(uploadId);
            postFile.DemoLookups = postFile.Demos.Select(d => allDemos[d]).ToList();

            return postFile;
        }

        public PostPrePostingFileSettings GetPostSettings(int uploadId)
        {
            var allDemos = _AudiencesCache.GetAllLookups().ToDictionary(l => l.Id);

            var postFile = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().GetPostSettings(uploadId);
            postFile.DemoLookups = postFile.Demos.Select(d => allDemos[d]).ToList();

            return postFile;
        }

        public bool DeletePost(int id)
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().DeletePost(id);
        }
    }
}
