using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters.Post;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostPrePostingService : IApplicationService
    {
        int SavePost(PostRequest request);
        List<PostPrePostingFile> GetPosts();
        PostPrePostingFile GetPost(int uploadId);
        bool DeletePost(int id);
        ReportOutput GenerateReportWithImpression(int id);
        int EditPost(PostRequest request);
        PostPrePostingDto GetInitialData();
    }

    public class PostPrePostingService : IPostPrePostingService
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
            if (!request.FileName.EndsWith(".xlsx"))
                throw new ApplicationException(FileNotExcelErroMessage);

            ValidateRequest(request);

            using (var excelPackage = new ExcelPackage(request.PostStream))
            {
                var postFileParser = _PostFileParserFactory.CreateParser(excelPackage);

                var postFileDetails = postFileParser.ParseExcel(excelPackage);

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

                var id = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().SavePost(postFile);

                _PostEngine.Post(postFile);

                return id;
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
            var post = GetPost(id);
            return _PostReportGenerator.Generate(post);
        }

        public List<PostPrePostingFile> GetPosts()
        {
            var allDemos = _AudiencesCache.GetAllLookups().ToDictionary(l => l.Id);

            var postFiles = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().GetAllPostsList();
            postFiles.ForEach(pf => pf.DemoLookups = pf.Demos.Select(d => allDemos[d]).ToList());

            return postFiles;
        }

        public PostPrePostingFile GetPost(int uploadId)
        {
            var allDemos = _AudiencesCache.GetAllLookups().ToDictionary(l => l.Id);

            var postFile = _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().GetPost(uploadId);
            postFile.DemoLookups = postFile.Demos.Select(d => allDemos[d]).ToList();

            return postFile;
        }

        public bool DeletePost(int id)
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IPostPrePostingRepository>().DeletePost(id);
        }
    }
}
