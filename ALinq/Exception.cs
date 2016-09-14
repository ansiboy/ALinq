using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq
{
    /// <summary>
    /// Represents errors that occur when an attempt is made to change a foreign key when the entity is already loaded.
    /// </summary>
    public class ForeignKeyReferenceAlreadyHasValueException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.ForeignKeyReferenceAlreadyHasValueException class with a system-supplied message that describes the error.
        /// </summary>
        public ForeignKeyReferenceAlreadyHasValueException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.ForeignKeyReferenceAlreadyHasValueException class with a specified message that describes the error.
        /// </summary>
        /// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
        public ForeignKeyReferenceAlreadyHasValueException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.ForeignKeyReferenceAlreadyHasValueException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public ForeignKeyReferenceAlreadyHasValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
 

}
