using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Reflection;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/health")]
	public class HealthApiController : BroadcastControllerBase
	{
		#region constructor

		public HealthApiController(
			BroadcastApplicationServiceFactory applicationServiceFactory)
			: base(new ControllerNameRetriever(typeof(HealthApiController).Name), applicationServiceFactory)
		{
		}

		#endregion

		#region api calls

		[HttpGet]
		public BaseResponse<HealthResponseDto> Get()
		{
			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				return _ConvertToBaseResponse(() =>
					_ApplicationServiceFactory.GetApplicationService<IHealthService>().GetInfo(assembly)
				);
			}
			catch (Exception e)
			{
				return new BaseResponse<HealthResponseDto>
				{
					Success = false,
					Message = e.Message
				};
			}
		}

        #endregion
	}
}