using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Web;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [DataContract]
    public class BaseResponseWithProblems<TResponce,TProblem> : BaseResponse<TResponce>
    {
        [DataMember]
        public List<TProblem> Problems { get; set; }
    }

    //[EnableCors(origins: "*", headers: "*", methods: "*")] //CORS enabled in global.asax file
    public class BroadcastControllerBase : ControllerBase
    {
        protected readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public BroadcastControllerBase(ControllerNameRetriever controllerNameRetriever, BroadcastApplicationServiceFactory applicationServiceFactory) 
            : base(controllerNameRetriever)
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        public WindowsIdentity Identity
        {
            get { return HttpContext.Current.Request.LogonUserIdentity; }
        }

        protected string _GetCurrentUserFullName() =>
            _ApplicationServiceFactory.GetApplicationService<IUserService>().GetCurrentUserFullName();
    }
}