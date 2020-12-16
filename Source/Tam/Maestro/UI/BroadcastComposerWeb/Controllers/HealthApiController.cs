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
				return new BaseResponse<HealthResponseDto>
				{
					Success = false,
					Message = e.Message
				};
			}
		}

		[Route("test-apm0")]
		[HttpGet]
		public BaseResponse<string> TestAPM0()
		{
			var message = string.Empty;
			using (var scope = Tracer.Instance.StartActive("TestAPM0"))
			{
				var span = scope.Span;

				// Always keep this trace
				span.SetTag(Tags.ManualKeep, "true");

				//method impl follows

			}
			message = "Hello!  I've reported metric 'BroadcastFirstMetricTest.'";
			return _ConvertToBaseResponse(() => message);
		}

		[Route("test-apm1")]
		[HttpGet]
		public BaseResponse<string> TestAPM1(int number)
		{
			Thread.Sleep(number * 1000);
			var message = $"Hello!  Total time for this call is {number} seconds!";
			return _ConvertToBaseResponse(() => message);
		}

		[Route("test-apm2")]
		[HttpGet]
		public BaseResponse<string> TestAPM2(bool throwError = false)
        {
            return _ConvertToBaseResponse(() =>
            {
                if (throwError)
                {
                    throw new ApplicationException($"Throwing because '{nameof(throwError)}' is true.");
                }

                return $"All is well.  '{nameof(throwError)}' is false.";
            });
        }

        [Route("test-apm3")]
        [HttpGet]
        public string TestAPM3(bool throwError = false)
        {
			if (throwError)
            {
                throw new ApplicationException($"Throwing because '{nameof(throwError)}' is true.");
            }

            return $"All is well.  '{nameof(throwError)}' is false.";
		}

		#endregion
	}
}