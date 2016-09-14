using System;

namespace ALinq.Mapping
{
    /// <summary>
    /// Used to specify each type of result; for functions having various result types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ResultTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.ResultTypeAttribute class.
        /// </summary>
        /// <param name="type">The type of the result returned by a function having various result types.</param>
        public ResultTypeAttribute(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets the valid or expected type mapping for a function having various result types.
        /// </summary>
        /// <returns>
        /// The type of result (System.Type).
        /// </returns>
        public Type Type { get; private set; }
    }
}