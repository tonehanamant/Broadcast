using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters;
using Services.Broadcast.Converters.Post;
using Services.Broadcast.Entities;
using Services.Broadcast.ReportGenerators;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostService : IApplicationService
    {
        int SavePost(PostRequest request);
        List<PostFile> GetPosts();
        PostFile GetPost(int uploadId);
        bool DeletePost(int id);
        ReportOutput GenerateReportWithImpression(int id);
        int EditPost(PostRequest request);
        PostDto GetInitialData();
    }

    public class PostService : IPostService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IPostEngine _PostEngine;
        private readonly IPostFileParserFactory _PostFileParserFactory;
        private readonly IReportGenerator<PostFile> _PostReportGenerator;
        private readonly IRatingForecastService _RatingForecastService;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        internal static readonly string InvalidPlaybackTypeErrorMessage = "PlaybackType {0} is not valid.";
        internal static readonly string FileNotExcelErroMessage = "File does not end with '.xlsx'.";
        internal static readonly string NoAudiencesErrorMessage = "Must have at least one Audience.";
        internal static readonly string MissingId = "File missing Id.";
        internal static readonly string DuplicateFileErrorMessage = "Could not import file, it has been imported. Delete existing file if needed";

        public PostService(IDataRepositoryFactory broadcastDataRepositoryFactory, IPostEngine postEngine, IPostFileParserFactory postFileParserFactoryFactory, IReportGenerator<PostFile> postReportGenerator, IRatingForecastService ratingForecastService, IBroadcastAudiencesCache audiencesCache)
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
                    playback_type = (byte) request.PlaybackType,
                    file_name = request.FileName,
                    upload_date = DateTime.Now,
                    modified_date = DateTime.Now,
                    post_file_details = postFileDetails,
                    post_file_demos = request.Audiences.Select(a => new post_file_demos {demo = a}).ToList()
                };

                var id = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>().SavePost(postFile);

                _PostEngine.Post(postFile);

                return id;
            }
        }

        public int EditPost(PostRequest request)
        {
            if (!request.FileId.HasValue)
                throw new ApplicationException(MissingId);

            ValidateRequest(request);

            _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>().DeletePostImpressions(request.FileId.Value);

            var file = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>().GetPostEF(request.FileId.Value);
            //Check that this removes the old demos then add new ones instead of just adding demos
            file.post_file_demos = request.Audiences.Select(a => new post_file_demos { demo = a }).ToList();
            file.equivalized = request.Equivalized;
            file.playback_type = (byte)request.PlaybackType;
            file.posting_book_id = request.PostingBookId;
            file.modified_date = DateTime.Now;

            var id = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>().SavePost(file);

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

            if (_BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>().PostExist(request.FileName))
                throw new ApplicationException(string.Format(DuplicateFileErrorMessage, request.FileName));
        }

        public PostDto GetInitialData()
        {
            var dto = new PostDto();
            dto.PostingBooks = _RatingForecastService.GetMediaMonthCrunchStatuses().Where(m => m.Crunched == CrunchStatus.Crunched).Select(m => new LookupDto(m.MediaMonth.Id, m.MediaMonth.MediaMonthX)).ToList();
            dto.PlaybackTypes = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalPlaybackType>();
            dto.Demos = _AudiencesCache.GetAllLookups();
            return dto;
        }

        public ReportOutput GenerateReportWithImpression(int id)
        {
            var post = GetPost(id);
            return _PostReportGenerator.Generate(post);
        }

        public List<PostFile> GetPosts()
        {
            var allDemos = _AudiencesCache.GetAllLookups().ToDictionary(l => l.Id);

            var postFiles = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>().GetAllPostsList();
            postFiles.ForEach(pf => pf.DemoLookups = pf.Demos.Select(d => allDemos[d]).ToList());

            return postFiles;
        }

        public PostFile GetPost(int uploadId)
        {
            var allDemos = _AudiencesCache.GetAllLookups().ToDictionary(l => l.Id);

            var postFile = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>().GetPost(uploadId);
            postFile.DemoLookups = postFile.Demos.Select(d => allDemos[d]).ToList();

            return postFile;
        }

        public bool DeletePost(int id)
        {
            return _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>().DeletePost(id);
        }
    }
}
