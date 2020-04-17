using BroadcastLogging;
using log4net;
using System;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;

namespace Tam.Maestro.Web.Common
{
    public class ControllerNameRetriever
    {
        private readonly string _ControllerName;

        public ControllerNameRetriever(string controllerName)
        {
            _ControllerName = controllerName;
        }

        public virtual string _GetControllerName()
        {
            return _ControllerName;
        }
    }

    public class ControllerBase : ApiController
    {
        protected readonly ILog _Log;
        private readonly ControllerNameRetriever _ControllerNameRetriever;

        public ControllerBase(ControllerNameRetriever controllerNameRetriever)
        {
            _Log = LogManager.GetLogger(typeof(ControllerBase));
            _ControllerNameRetriever = controllerNameRetriever;
        }

        protected virtual void _ThrowIfModelStateInvalid()
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("The ModelState was invalid.");
            }
        }

        protected virtual BaseResponse<T> _ConvertToBaseResponse<T>(Func<T> func)
        {
            try
            {
                _ThrowIfModelStateInvalid();
                var response = func();
                return (new BaseResponse<T>
                {
                    Success = true,
                    Message = null,
                    Data = response
                });
            }
            catch (Exception e)
            {
                _LogError($"Exception from url '{HttpContext.Current.Request.RawUrl}'", e, _ControllerNameRetriever._GetControllerName());

                return (new BaseResponse<T>
                {
                    Success = false,
                    Message = e.Message,
                    Data = default(T)
                });
            }
        }

        protected virtual BaseResponseWithStackTrace<T> _ConvertToBaseResponseWithStackTrace<T>(Func<T> func)
        {
            try
            {
                _ThrowIfModelStateInvalid();
                var response = func();
                return (new BaseResponseWithStackTrace<T>
                {
                    Success = true,
                    Message = null,
                    Data = response
                });
            }
            catch (Exception e)
            {
                _LogError($"Exception caught.", e, _ControllerNameRetriever._GetControllerName());

                return new BaseResponseWithStackTrace<T>
                {
                    Success = false,
                    Message = e.Message,
                    Data = default(T),
                    StackTrace = e.StackTrace
                };
            }
        }

        protected virtual BaseResponse<T> _ConvertToBaseResponseSuccessWithMessage<T>(Func<Tuple<T, string>> func)
        {
            try
            {
                _ThrowIfModelStateInvalid();
                var response = func();
                return (new BaseResponse<T>
                {
                    Success = true,
                    Message = response.Item2,
                    Data = response.Item1
                });
            }
            catch (Exception e)
            {
                _LogError($"Exception caught.", e, _ControllerNameRetriever._GetControllerName());

                return (new BaseResponse<T>
                {
                    Success = false,
                    Message = e.Message,
                    Data = default(T)
                });
            }
        }

        protected void _LogError(string message, Exception ex, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
        }
    }
}