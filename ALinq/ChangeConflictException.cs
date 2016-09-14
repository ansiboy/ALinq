using System;

namespace ALinq
{
    /// <summary>
    /// 
    /// </summary>
    public class ChangeConflictException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.ChangeConflictException class.
        /// </summary>
        public ChangeConflictException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.ChangeConflictException class and specifies a message to explain the exception.
        /// </summary>
        /// <param name="message">The message to be exposed when the exception is thrown.</param>
        public ChangeConflictException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.ChangeConflictException class, specifies a message to explain the exception, and specifies the exception that caused this exception.
        /// </summary>
        /// <param name="message">The message to be exposed when the exception is thrown.</param>
        /// <param name="innerException">Specifies the exception of which ALinq.ChangeConflictException is a result.</param>
        public ChangeConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}