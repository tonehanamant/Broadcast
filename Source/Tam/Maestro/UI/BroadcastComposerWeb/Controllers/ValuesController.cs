using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Common.Services.WebComponents;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

using Services.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Repositories;
using Services.Broadcast.Converters;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class ValuesController : ControllerBase
    {
        private readonly Services.Broadcast.ApplicationServices.BroadcastServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public ValuesController(IWebLogger logger, BroadcastServiceFactory factory)
            : base(logger, new ControllerNameRetriever("ValuesController"))
        {
            _Logger = logger;
            _ApplicationServiceFactory = factory;
        }
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }


        [Route("UploadSchedule")]
        [HttpPost]
        public System.Web.Mvc.JsonResult ScheduleSaveRequest()
        {
            System.Web.HttpPostedFile BVSFile = System.Web.HttpContext.Current.Request.Files.Count > 0 ?
                System.Web.HttpContext.Current.Request.Files[0] : null;
            var result = new System.Web.Mvc.JsonResult();
            try
            {
                if (BVSFile != null && BVSFile.ContentLength > 0)
                {
                    String fileName = System.IO.Path.GetFileName(BVSFile.FileName).Trim();
                    String fileNameExtension = System.IO.Path.GetExtension(BVSFile.FileName);
                    byte[] BVSBytes = new byte[BVSFile.InputStream.Length];
                    BVSFile.InputStream.Read(BVSBytes, 0, (int)BVSFile.InputStream.Length);
                    // if(_ApplicationServiceFactory.GetApplicationService<Services.Broadcast.ApplicationServices.ITrackerService>() == SUCCESS){
                    //      result.Data = new {success="true"};
                    // } else {
                    //      result.Data = new {success="false", error="The application was unable to process your file"};
                    // }
                }
                else
                {
                    result.Data = new {success="false", error="The file is empty."};
                }
            }
            catch (Exception ex)
            {
                _Logger.LogExceptionWithServiceName(ex, "ValuesController");
                result.Data = new { success = "false", error = "There was an exception processing your file" };
            }
            result.JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.DenyGet;
            return result;
        }
    }
}
