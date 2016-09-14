using System;
using System.Reflection;

namespace ALinq.Mapping
{
    /// <summary>
    /// Represents the mapping between a method parameter and a database function parameter.
    /// </summary>
    public abstract class MetaParameter
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.MetaParameter class.
        /// </summary>
        protected MetaParameter()
        {

        }

        /// <summary>
        /// Gets the database type of the parameter.
        /// </summary>
        /// <returns>
        /// The database type of the parameter as a string.
        /// </returns>
        public abstract string DbType { get; }

        /// <summary>
        /// Gets the name of the parameter in the database function.
        /// </summary>
        /// <returns>
        /// The name as a string.
        /// </returns>
        public abstract string MappedName { get; }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <returns>The name of the parameter as a string.</returns>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the underlying method parameter.
        /// </summary>
        /// <returns>The underlying method parameter.</returns>
        public abstract ParameterInfo Parameter { get; }

        /// <summary>
        /// Gets the common language runtime (CLR) type of the parameter.
        /// </summary>
        /// <returns>The type.</returns>
        public abstract Type ParameterType { get; }
    }



}