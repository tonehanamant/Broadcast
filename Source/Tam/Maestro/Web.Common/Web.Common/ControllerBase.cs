using Common.Services.WebComponents;
using System;
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
        private readonly IWebLogger _Logger;
        private readonly ControllerNameRetriever _ControllerNameRetriever;

        public ControllerBase(IWebLogger logger, ControllerNameRetriever controllerNameRetriever)
        {
            _Logger = logger;
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
                _Logger.LogExceptionWithServiceName(e, _ControllerNameRetriever._GetControllerName());

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
                _Logger.LogExceptionWithServiceName(e, _ControllerNameRetriever._GetControllerName());

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
                _Logger.LogExceptionWithServiceName(e, _ControllerNameRetriever._GetControllerName());

                return (new BaseResponse<T>
                {
                    Success = false,
                    Message = e.Message,
                    Data = default(T)
                });
            }
        }
    }
}