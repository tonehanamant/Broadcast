using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Files")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class FileApiController : BroadcastControllerBase
    {
        public FileApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(ContainTypeApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets the file by id
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <param name="shouldBeRemoved">Indicates whether a file should be removed after retrieving. By default it is set to true</param>
        /// <returns>File content</returns>
        [HttpGet]
        [Route("{fileId}")]
        public HttpResponseMessage GetFile(Guid fileId, bool shouldBeRemoved = true)
        {
            var file = shouldBeRemoved ?
                _ApplicationServiceFactory.GetApplicationService<ISharedFolderService>().GetAndRemoveFile(fileId) :
                _ApplicationServiceFactory.GetApplicationService<ISharedFolderService>().GetFile(fileId);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(file.FileContent);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(file.FileMediaType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = file.FileNameWithExtension
            };

            return result;
        }
    }
}
