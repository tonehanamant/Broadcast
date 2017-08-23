using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Tam.Maestro.Data.Entities
{
    /// <summary>
    /// Add ExceptionType and ExceptionData if needed.
    /// </summary>
    public class TAMResultExp : Exception
    {
        private string _message;

        public ResultStatus Status { get; set; }

        public override string Message { get { return _message; } }

        public TAMResultExp(string message, ResultStatus status = ResultStatus.Error)
        {
            _message = message;
            Status = status;
        }
        
    }


    [DataContract]
    public class TAMResult2<T>
    {
        [DataMember]
        public ResultStatus Status;
        [DataMember]
        public T Result;
        [DataMember]
        public string Message;
        [DataMember]
        public ExceptionType ExceptionType;
        [DataMember]
        public List<string> ExceptionData;

        public TAMResult2()
        {
            Status = ResultStatus.Success;
            Message = "";
            ExceptionData = new List<string>();
        }
        public TAMResult2(ResultStatus pStatus)
        {
            Status = pStatus;
            Message = "";
            ExceptionData = new List<string>();
        }
        public TAMResult2(T t)  //Assume success
        {
            Result = t;
            Status = ResultStatus.Success;
            Message = "";
            ExceptionData = new List<string>();
        }
        public TAMResult2(T t, ResultStatus pStatus)
        {
            Result = t;
            Status = pStatus;
            Message = "";
            ExceptionData = new List<string>();
        }
        public TAMResult2(ResultStatus pStatus, string pMessage)
        {
            Status = pStatus;
            Message = pMessage;
            ExceptionData = new List<string>();
        }
        public TAMResult2(T t, ResultStatus pStatus, string pMessage)
        {
            Status = pStatus;
            Message = pMessage;
            Result = t;
            ExceptionData = new List<string>();
        }
        public TAMResult2(Exception exc, string context)
        {
            var msg = new StringBuilder();
            msg.AppendLine(context);
            msg.AppendLine(exc.Message);

            var exc1 = exc.InnerException;
            while (exc1 != null)
            {
                msg.AppendLine(exc1.Message);
                exc1 = exc1.InnerException;
            }

            Status = ResultStatus.Error;
            Message = msg.ToString();
            ExceptionData = new List<string>();
        }
    }
    [DataContract]
    public class TAMResult2<T, U>
    {
        [DataMember]
        public ResultStatus Status;
        [DataMember]
        public T Result1;
        [DataMember]
        public U Result2;
        [DataMember]
        public string Message;

        public TAMResult2()
        {
            Status = ResultStatus.Success;
            Message = "";
        }
        public TAMResult2(ResultStatus pStatus)
        {
            Status = pStatus;
            Message = "";
        }
        public TAMResult2(T t, U u)  //Assume success
        {
            Result1 = t;
            Result2 = u;
            Status = ResultStatus.Success;
            Message = "";
        }
        public TAMResult2(T t, U u, ResultStatus pStatus)
        {
            Result1 = t;
            Result2 = u;
            Status = pStatus;
            Message = "";
        }
        public TAMResult2(ResultStatus pStatus, string pMessage)
        {
            Status = pStatus;
            Message = pMessage;
        }

        public TAMResult2(Exception exc, string context)
        {
            var msg = new StringBuilder();
            msg.AppendLine(context);
            msg.AppendLine(exc.Message);

            var exc1 = exc.InnerException;
            while (exc1 != null)
            {
                msg.AppendLine(exc1.Message);
                exc1 = exc1.InnerException;
            }

            Status = ResultStatus.Error;
            Message = msg.ToString();
        }
    }
    [DataContract]
    public class TAMResult2<T, U, V>
    {
        [DataMember]
        public ResultStatus Status;
        [DataMember]
        public T Result1;
        [DataMember]
        public U Result2;
        [DataMember]
        public V Result3;
        [DataMember]
        public string Message;

        public TAMResult2()
        {
            Status = ResultStatus.Success;
            Message = "";
        }
        public TAMResult2(ResultStatus pStatus)
        {
            Status = pStatus;
            Message = "";
        }
        public TAMResult2(T t, U u, V v)  //Assume success
        {
            Result1 = t;
            Result2 = u;
            Result3 = v;
            Status = ResultStatus.Success;
            Message = "";
        }
        public TAMResult2(T t, U u, V v, ResultStatus pStatus)
        {
            Result1 = t;
            Result2 = u;
            Result3 = v;
            Status = pStatus;
            Message = "";
        }
        public TAMResult2(ResultStatus pStatus, string pMessage)
        {
            Status = pStatus;
            Message = pMessage;
        }

        public TAMResult2(Exception exc, string context)
        {
            var msg = new StringBuilder();
            msg.AppendLine(context);
            msg.AppendLine(exc.Message);

            var exc1 = exc.InnerException;
            while (exc1 != null)
            {
                msg.AppendLine(exc1.Message);
                exc1 = exc1.InnerException;
            }

            Status = ResultStatus.Error;
            Message = msg.ToString();
        }
    }
}
