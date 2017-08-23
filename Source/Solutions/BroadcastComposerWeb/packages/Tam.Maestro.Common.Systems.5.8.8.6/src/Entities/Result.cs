using System.Runtime.Serialization;

namespace Tam.Maestro.Data.Entities
{
    [DataContract]
	public class TAMResult
    {
        [DataMember]
        public ResultStatus Type;
        [DataMember]
        public object SingleResult;
        [DataMember]
        public object[] MultipleResult;
        [DataMember]
        public string Message;

        public TAMResult()
        {
            this.Type = ResultStatus.Success;
            this.SingleResult = null;
            this.MultipleResult = null;
            this.Message = "";
        }
        public TAMResult(ResultStatus pStatus)
        {
            this.Type = pStatus;
            this.SingleResult = null;
            this.MultipleResult = null;
            this.Message = "";
        }

        public TAMResult(ResultStatus pStatus, object pResult, object[] pMResult, string pMessage)
        {
            this.Type = pStatus;
            this.SingleResult = pResult;
            this.MultipleResult = pMResult;
            this.Message = pMessage;
        }
    }
}
