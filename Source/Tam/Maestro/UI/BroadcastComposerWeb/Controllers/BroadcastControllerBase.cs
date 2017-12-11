using System.Security.Principal;
using Tam.Maestro.Web.Common;
using System.Web;
using Common.Services.WebComponents;

namespace BroadcastComposerWeb.Controllers
{
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