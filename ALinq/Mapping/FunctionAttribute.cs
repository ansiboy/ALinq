using System;

namespace ALinq.Mapping
{
    /// <summary>
    /// Associates a method with a stored procedure or user-defined function in the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.FunctionAttribute class.
        /// </summary>
        public FunctionAttribute()
        {
            
        }

        /// <summary>
        /// Gets or sets whether a method is mapped to a function or to a stored procedure.
        /// </summary>
        /// <returns>true if a function; false if a stored procedure.</returns>
        public bool IsComposable { get; set; }

        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        /// <returns>
        /// The name of the function or stored procedure.
        /// </returns>
        public string Name { get; set; }
    }
}