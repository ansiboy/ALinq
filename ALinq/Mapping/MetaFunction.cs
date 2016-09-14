using System.Collections.ObjectModel;
using System.Reflection;

namespace ALinq.Mapping
{
    /// <summary>
    /// Represents the mapping between a context method and a database function.
    /// </summary>
    public abstract class MetaFunction
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.MetaFunction class.
        /// </summary>
        protected MetaFunction()
        {
        }

        /// <summary>
        /// Gets whether or not the stored procedure has multiple result types.
        /// </summary>
        /// <returns>
        /// true if the stored procedure has multiple result types.
        /// </returns>
        public abstract bool HasMultipleResults { get; }

        /// <summary>
        /// Gets whether the function can be composed within a query.
        /// </summary>
        /// <returns>
        /// true if the function can be composed within a query.
        /// </returns>
        public abstract bool IsComposable { get; }

        /// <summary>
        /// Gets the name of the database function or procedure.
        /// </summary>
        /// <returns>
        /// A string representing the name of the database function or procedure.
        /// </returns>
        public abstract string MappedName { get; }

        /// <summary>
        /// Gets the underlying context method.
        /// </summary>
        /// <returns>
        /// A System.Reflection.MethodInfo object that corresponds with the underlying context method.
        /// </returns>
        public abstract MethodInfo Method { get; }

        /// <summary>
        /// Gets the ALinq.Mapping.MetaModel that contains this function.
        /// </summary>
        /// <returns>
        /// The ALinq.Mapping.MetaModel object that contains this function.
        /// </returns>
        public abstract MetaModel Model { get; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        /// <returns>
        /// A string that represents the name of the method.
        /// </returns>
        public abstract string Name { get; }

        /// <summary>
        /// Gets an enumeration of the function parameters.
        /// </summary>
        /// <returns>
        /// A collection of the parameters.
        /// </returns>
        public abstract ReadOnlyCollection<MetaParameter> Parameters { get; }

        /// <summary>
        /// Gets the enumeration of possible result row types.
        /// </summary>
        /// <returns>
        /// A collection of possible types.
        /// </returns>
        public abstract ReadOnlyCollection<MetaType> ResultRowTypes { get; }

        /// <summary>
        /// Gets the return parameter.
        /// </summary>
        /// <returns>
        /// The ALinq.Mapping.MetaParameter that corresponds to the return parameter.
        /// </returns>
        public abstract MetaParameter ReturnParameter { get; }
    }




}
