using BroadcastLogging;
using log4net;
using Services.Broadcast.Exceptions;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
                throw new CadentException("The ModelState was invalid.");
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

                if (e.GetBaseException() is CadentException)
                {
                    if (e.GetType() == typeof(AggregateException) && e.InnerException != null)
                    {
                        return new BaseResponse<T>
                        {
                            Success = false,
                            Message = e.InnerException.Message,
                            Data = default(T)
                        };
                    }

                    return new BaseResponse<T>
                    {
                        Success = false,
                        Message = e.Message,
                        Data = default(T)
                    };
                }
                else
                {
                    return new BaseResponse<T>
                    {
                        Success = false,
                        Message = "Call your administrator to check the log messages.",
                        Data = default(T)
                    };
                }
            }
        }

        protected async virtual Task<BaseResponse<T>> _ConvertToBaseResponseAsync<T>(Func<Task<T>> func)
        {
            try
            {
                _ThrowIfModelStateInvalid();
                var response = await func();
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

                if (e.GetBaseException() is CadentException)
                {
                    if (e.GetType() == typeof(AggregateException) && e.InnerException != null)
                    {
                        return new BaseResponse<T>
                        {
                            Success = false,
                            Message = e.InnerException.Message,
                            Data = default(T)
                        };
                    }

                    return new BaseResponse<T>
                    {
                        Success = false,
                        Message = e.Message,
                        Data = default(T)
                    };
                } 
                else
                {
                    return new BaseResponse<T>
                    {
                        Success = false,
                        Message = "Call your administrator to check the log messages.",
                        Data = default(T)
                    };
                }
            }
        }

        protected async virtual Task<BaseResponseWithStackTrace<T>> _ConvertToBaseResponseWithStackTraceAsync<T>(Func<Task<T>> func)
        {
            try
            {
                _ThrowIfModelStateInvalid();
                var response = await func();
                return new BaseResponseWithStackTrace<T>
                {
                    Success = true,
                    Message = null,
                    Data = response
                };
            }
            catch (Exception e)
            {
                _LogError($"Exception caught.", e, _ControllerNameRetriever._GetControllerName());

                if (e.GetBaseException() is CadentException)
                {
                    if (e.GetType() == typeof(AggregateException) && e.InnerException != null)
                    {
                        return new BaseResponseWithStackTrace<T>
                        {
                            Success = false,
                            Message = e.InnerException.Message,
                            Data = default(T),
                            StackTrace = e.StackTrace
                        };
                    }

                    return new BaseResponseWithStackTrace<T>
                    {
                        Success = false,
                        Message = e.Message,
                        Data = default(T),
                        StackTrace = e.StackTrace
                    };
                }
                else
                {
                    return new BaseResponseWithStackTrace<T>
                    {
                        Success = false,
                        Message = "Call your administrator to check the log messages.",
                        Data = default(T),
                        StackTrace = e.StackTrace
                    };
                }
            }


        }
        

        protected virtual BaseResponseWithStackTrace<T> _ConvertToBaseResponseWithStackTrace<T>(Func<T> func)
        {
            try
            {
                _ThrowIfModelStateInvalid();
                var response = func();
                return new BaseResponseWithStackTrace<T>
                {
                    Success = true,
                    Message = null,
                    Data = response
                };
            }
            catch (Exception e)
            {
                _LogError($"Exception caught.", e, _ControllerNameRetriever._GetControllerName());

                if (e.GetBaseException() is CadentException)
                {
                    if (e.GetType() == typeof(AggregateException) && e.InnerException != null)
                    {
                        return new BaseResponseWithStackTrace<T>
                        {
                            Success = false,
                            Message = e.InnerException.Message,
                            Data = default(T),
                            StackTrace = e.StackTrace
                        };
                    }

                    return new BaseResponseWithStackTrace<T>
                    {
                        Success = false,
                        Message = e.Message,
                        Data = default(T),
                        StackTrace = e.StackTrace
                    };
                } else
                {
                    return new BaseResponseWithStackTrace<T>
                    {
                        Success = false,
                        Message = "Call your administrator to check the log messages.",
                        Data = default(T),
                        StackTrace = e.StackTrace
                    };
                }
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

        protected void _LogInfo(string message, string username = null, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName, username);
            _Log.Info(logMessage.ToJson());
        }

        protected void _LogError(string message, Exception ex, [CallerMemberName] string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
        }
    }
}