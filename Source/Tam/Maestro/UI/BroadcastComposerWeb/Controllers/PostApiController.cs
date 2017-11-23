using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;
using ControllerBase = Tam.Maestro.Web.Common.ControllerBase;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/Post")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class PostApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public PostApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever("PostController"))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        [HttpPost]
        [Route("")]
        public BaseResponse<int> Post(HttpRequestMessage saveRequest)
        {
            if (saveRequest == null)
                throw new Exception("No post file data received.");

            var request = JsonConvert.DeserializeObject<PostRequest>(saveRequest.Content.ReadAsStringAsync().Result);
            request.UserName = Identity.Name;
            request.UploadDate = DateTime.Now;
            request.ModifiedDate = DateTime.Now;
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostService>().SavePost(request));
        }

        [HttpPut]
        [Route("")]
        public BaseResponse<int> EditPost(HttpRequestMessage saveRequest)
        {
            if (saveRequest == null)
                throw new Exception("No post file data received.");

            var request = JsonConvert.DeserializeObject<PostRequest>(saveRequest.Content.ReadAsStringAsync().Result);
            if (request.FileId == null)
                throw new Exception("Can not edit post with no id.");

            request.UserName = Identity.Name;
            request.ModifiedDate = DateTime.Now;

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostService>().EditPost(request));
        }

        [HttpGet]
        [Route("Report/{id}")]
        public HttpResponseMessage GeneratePostReport(int id)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IPostService>().GenerateReportWithImpression(id);
            var result = Request.CreateResponse(HttpStatusCode.OK);

            result.Content = new StreamContent(report.Stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = report.Filename
            };

            return result;
        }

        [HttpGet]
        [Route("")]
        public BaseResponse<List<PostFile>> GetPosts()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostService>().GetPosts());
        }

        [HttpGet]
        [Route("{id}")]
        public BaseResponse<PostFile> GetPost(int id)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostService>().GetPost(id));
        }

        [HttpDelete]
        [Route("{id}")]
        public BaseResponse<bool> DeletePost(int id)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostService>().DeletePost(id));
        }

        [HttpGet]
        [Route("InitialData")]
        public BaseResponse<PostDto> GetInitialData()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostService>().GetInitialData());
        }
        }
}