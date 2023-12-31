﻿using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    /// <summary>
    /// Operations for Daypart types.
    /// </summary>
    /// <seealso cref="BroadcastComposerWeb.Controllers.BroadcastControllerBase" />
    [RoutePrefix("api/v1/DaypartTypes")]
    public class DaypartTypeApiController : BroadcastControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaypartTypeApiController"/> class.
        /// </summary>
        /// <param name="applicationServiceFactory">The application service factory.</param>
        public DaypartTypeApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(DaypartTypeApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets the types of dayparts.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<LookupDto>> GetDaypartTypes()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IDaypartTypeService>().GetDaypartTypes());
        }
    }
}