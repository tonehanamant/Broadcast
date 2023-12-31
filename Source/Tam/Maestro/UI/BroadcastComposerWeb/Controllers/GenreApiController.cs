﻿using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Genres")]
    public class GenreApiController : BroadcastControllerBase
    {
        public GenreApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(GenreApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Endpoint for listing the genres in the system.
        /// </summary>
        /// <param name="sourceId">The source id (1 - Maestro and 2 - Dativa). Default value is 1.</param>
        /// <returns>List of genres</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<LookupDto>> GetGenres(int sourceId = (int)ProgramSourceEnum.Maestro)
        {
            return _ConvertToBaseResponse(() =>
                 _ApplicationServiceFactory.GetApplicationService<IGenreService>().GetGenres(sourceId)
            );
        }
    }
}