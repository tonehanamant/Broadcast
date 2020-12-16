using System;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using Datadog.Trace;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
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
				return new BaseResponse<HealthResponseDto>()
				{
					Success = false,
					Message = e.Message
				};
			}
			
		}
		[Route("test-apm")]
		[HttpGet]
		public BaseResponse<bool> TestAPM()
		{
			using (var scope = Tracer.Instance.StartActive("TestAPM"))
			{
				var span = scope.Span;

				// Always keep this trace
				span.SetTag(Tags.ManualKeep, "true");

				//method impl follows

			}

			return _ConvertToBaseResponse(() => true);
		}
		[Route("test-apm2")]
		[HttpGet]
		public BaseResponse<bool> TestAPM2()
		{
			using (var scope = Tracer.Instance.StartActive("TestAPM2"))
			{
				var span = scope.Span;

				// Always keep this trace
				span.SetTag(Tags.ManualKeep, "true");
				Random rnd = new Random();
				int number = rnd.Next(1, 5);
				Thread.Sleep(number*1000);
			}

			return _ConvertToBaseResponse(() => true);
		}
		#endregion
	}
}