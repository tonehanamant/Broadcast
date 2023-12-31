﻿using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
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

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/PostPrePosting")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class PostPrePostingApiController : BroadcastControllerBase
    {
        public PostPrePostingApiController(BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(PostPrePostingApiController).Name), applicationServiceFactory)
        {
        }

        [HttpPost]
        [Route("")]
        public BaseResponse<int> Post(HttpRequestMessage saveRequest)
        {
            if (saveRequest == null)
                throw new Exception("No post file data received.");

            var request = JsonConvert.DeserializeObject<PostRequest>(saveRequest.Content.ReadAsStringAsync().Result);
            request.UserName = _GetCurrentUserFullName();
            request.UploadDate = DateTime.Now;
            request.ModifiedDate = DateTime.Now;
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostPrePostingService>().SavePost(request));
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

            request.UserName = _GetCurrentUserFullName();
            request.ModifiedDate = DateTime.Now;

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostPrePostingService>().EditPost(request));
        }

        [HttpGet]
        [Route("Report/{id}")]
        public HttpResponseMessage GeneratePostReport(int id)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IPostPrePostingService>().GenerateReportWithImpression(id);
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
        [Route("Report_v2/{id}")]
        public HttpResponseMessage GeneratePostReportV2(int id)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IPostPrePostingService>().GenerateReportWithImpression_V2(id);
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
        [Route("Report_v3/{id}")]
        public HttpResponseMessage GeneratePostReportV3(int id)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IPostPrePostingService>().GenerateReportWithImpression_V3(id);
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
        public BaseResponse<List<PostPrePostingFile>> GetPosts()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostPrePostingService>().GetPosts());
        }

        [HttpGet]
        [Route("{id}")]
        public BaseResponse<PostPrePostingFileSettings> GetPost(int id)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostPrePostingService>().GetPostSettings(id));
        }

        [HttpDelete]
        [Route("{id}")]
        public BaseResponse<bool> DeletePost(int id)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostPrePostingService>().DeletePost(id));
        }

        [HttpGet]
        [Route("InitialData")]
        public BaseResponse<PostPrePostingDto> GetInitialData()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostPrePostingService>().GetInitialData());
        }
    }
}