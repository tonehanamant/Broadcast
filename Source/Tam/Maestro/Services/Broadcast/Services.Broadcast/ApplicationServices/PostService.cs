using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostService : IApplicationService
    {
        List<PostDto> GetPosts();
    }

    public class PostDto
    {
        public int ContractId { get; set; }
        public string ContractName { get; set; }
        public DateTime? UploadDate { get; set; }
        public int SpotsInSpec { get; set; }
        public int SpotsOutOfSpec { get; set; }
        public double? PrimaryAudienceImpressions { get; set; }
    }

    public class PostService : IPostService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IPostRepository _PostRepository;

        public PostService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _PostRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
        }

        public List<PostDto> GetPosts()
        {
            return _PostRepository.GetAllPostFiles();
        }
    }
}
