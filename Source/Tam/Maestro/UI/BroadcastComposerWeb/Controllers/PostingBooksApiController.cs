using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System.Web.Http;
using Tam.Maestro.Web.Common;
using Tam.Maestro.Services.Cable.Entities;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using System;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/PostingBooks")]
    public class PostingBooksApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public PostingBooksApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(PostingBooksApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        /// <summary>
        /// Gets all posting books.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<LookupDto>> GetAllPostingBooks()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostingBookService>().GetAllPostingBooks());
        }

        /// <summary>
        /// Gets the default share book id.
        /// </summary>
        /// <param name="startDate">The start date of the flight.</param>
        /// <returns>The default share book id</returns>
        [HttpGet]
        [Route("Share")]
        public BaseResponse<int> GetShareBooks(DateTime startDate)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostingBookService>().GetDefaultShareBookId(startDate));
        }

        /// <summary>
        /// Gets the hut books avaible based on share book id.
        /// </summary>
        /// <param name="shareBookId">Id of selected share book</param>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("HUT")]
        public BaseResponse<List<LookupDto>> GetHutBooks(int shareBookId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPostingBookService>().GetHUTBooks(shareBookId));
        }
    }
}
