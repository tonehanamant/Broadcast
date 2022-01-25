﻿using System;

namespace Services.Broadcast.Exceptions
{
    /// <summary>
    /// Exception describing an identified issue during the Plan Validation Operation.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class PlanValidationException : PlanSaveException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanValidationException"/> class.
        /// </summary>
        public PlanValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanSaveException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PlanValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference (<see langword="Nothing" /> in Visual Basic), the current exception is raised in a <see langword="catch" /> block that handles the inner exception.</param>
        public PlanValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}