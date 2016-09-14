using System;

namespace ALinq
{
    /// <summary>
    /// Thrown when an attempt is made to add an object to the identity cache by using a key that is already being used.
    /// </summary>
    public class DuplicateKeyException : InvalidOperationException
    {
        // Fields
        private object duplicate;

        /// <summary>
        /// Initializes a new instance of the ALinq.DuplicateKeyException class.
        /// </summary>
        /// <param name="duplicate">The duplicate key that caused the exception to be thrown.</param>
        public DuplicateKeyException(object duplicate)
        {
            this.duplicate = duplicate;
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.DuplicateKeyException class by referencing the duplicate key and providing an error message.
        /// </summary>
        /// <param name="duplicate">The duplicate key that caused the exception to be thrown.</param>
        /// <param name="message">The message to appear when the exception is thrown.</param>
        public DuplicateKeyException(object duplicate, string message)
            : base(message)
        {
            this.duplicate = duplicate;
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.DuplicateKeyException class by referencing the duplicate key, providing an error message, and specifying the exception that caused this exception to be thrown.
        /// </summary>
        /// <param name="duplicate">The duplicate key that caused the exception to be thrown.</param>
        /// <param name="message">The message to appear when the exception is thrown.</param>
        /// <param name="innerException">The previous exception that caused the ALinq.DuplicateKeyException exception to be thrown.</param>
        public DuplicateKeyException(object duplicate, string message, Exception innerException)
            : base(message, innerException)
        {
            this.duplicate = duplicate;
        }

        /// <summary>
        /// Gets the object that caused the exception.
        /// </summary>
        /// <returns>
        /// The object that caused the exception.
        /// </returns>
        public object Object
        {
            get
            {
                return this.duplicate;
            }
        }
    }
}