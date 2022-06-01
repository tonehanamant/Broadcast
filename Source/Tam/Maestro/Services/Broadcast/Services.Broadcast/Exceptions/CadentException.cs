using System;

namespace Services.Broadcast.Exceptions
{
    public class CadentException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="CadentException"/> class.
        /// </summary>
        public CadentException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="CadentException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CadentException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="CadentException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. 
        /// If the <paramref name="innerException" /> parameter is not a null reference 
        /// (<see langword="Nothing" /> in Visual Basic), the current exception is raised in a 
        /// <see langword="catch" /> block that handles the inner exception.</param>
        public CadentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}