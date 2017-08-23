using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces.Helpers
{
    [Serializable]
    public class TamValidationException : Exception
    {
        public List<string> ValidationErrors = new List<string>();

        public TamValidationException()
            : base()
        { }

        public TamValidationException(string message)
            : base(message)
        { }

        public TamValidationException(string message, IEnumerable<string> validationErrors)
            : base(message)
        {
            ValidationErrors.AddRange(validationErrors);
        }

        public TamValidationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public TamValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
