using Tam.Maestro.Services.Cable.Entities;

namespace Tam.Maestro.Web.Common
{
    public class BaseResponseWithStackTrace<T> : BaseResponse<T>
    {
        public string StackTrace { get; set; }
    }
}