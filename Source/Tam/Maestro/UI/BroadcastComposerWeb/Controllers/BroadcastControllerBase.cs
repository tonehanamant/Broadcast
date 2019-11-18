using System.Runtime.Serialization;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using Tam.Maestro.Web.Common;
using System.Web;
using Common.Services.WebComponents;
using Tam.Maestro.Services.Cable.Entities;
using System.Web.Http.Cors;

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
        public BroadcastControllerBase(IWebLogger logger, ControllerNameRetriever controllerNameRetriever) 
            : base(logger, controllerNameRetriever)
        {
        }

        public WindowsIdentity Identity
        {
            get { return HttpContext.Current.Request.LogonUserIdentity; }
        }

        public string FullName
        {
            get
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);

                var userName = string.IsNullOrEmpty(User.Identity.Name) ? 
                    WindowsIdentity.GetCurrent().Name : 
                    User.Identity.Name;

                _Logger.LogEventInformation(
                    message: $"User.Identity.Name: {User.Identity.Name}, WindowsIdentity.GetCurrent().Name: {WindowsIdentity.GetCurrent().Name}",
                    serviceName: "BroadcastControllerBase");

                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userName);

                return user.DisplayName;
            }
        }
    }
}