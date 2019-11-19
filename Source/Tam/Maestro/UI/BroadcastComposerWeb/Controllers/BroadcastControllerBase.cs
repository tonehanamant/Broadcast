using System.Runtime.Serialization;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using Tam.Maestro.Web.Common;
using System.Web;
using Common.Services.WebComponents;
using Tam.Maestro.Services.Cable.Entities;
using System.Web.Http.Cors;
using Tam.Maestro.Services.Clients;

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
                var ssid = HttpContext.Current.Request.LogonUserIdentity.User.Value;
                var employee = SMSClient.Handler.GetEmployee(ssid, false);
                if (employee == null)
                {
                    return null;
                }

                return employee.Employee.FullName;
            }
        }
    }
}