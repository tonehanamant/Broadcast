using System;
using System.Collections.Generic;

namespace Services.Broadcast.Exceptions
{
    public class FileUploadException<TProblem> : Exception
    {
        public List<TProblem> Problems { get; private set; }
        public FileUploadException(List<TProblem> problems)
        {
            Problems = problems;
        }
    }

}