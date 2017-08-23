using System.Runtime.Serialization;

namespace Tam.Maestro.Services.Cable.Entities
{
    [DataContract]
    public class BaseResponse<T>
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public T Data { get; set; }
    }

    [DataContract]
    public class BaseResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public object Data { get; set; }
    }
}