using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Security.Principal;
using Tam.Maestro.Web.Common;
using System.Web;
using Common.Services.WebComponents;
using Tam.Maestro.Services.Cable.Entities;

namespace BroadcastComposerWeb.Controllers
{
    [DataContract]
    public class BaseResponseWithProblems<TResponce,TProblem> : BaseResponse<TResponce>
    {
        [DataMember]
        public List<TProblem> Problems { get; set; }
    }

    public class BroadcastControllerBase : ControllerBase
    {
        public BroadcastControllerBase(IWebLogger logger, ControllerNameRetriever controllerNameRetriever) 
            : base(logger, controllerNameRetriever)
        {
        }

        public WindowsIdentity Identity
        {
            get { return HttpContext.Current.Request.LogonUserIdentity; }
        }
    }
}