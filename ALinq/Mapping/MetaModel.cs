using System;
using System.Collections.Generic;
using System.Reflection;

namespace ALinq.Mapping
{
    /// <summary>
    /// An abstraction that represents the mapping between a database and domain objects.
    /// </summary>
    public abstract class MetaModel
    {
        // Fields

        // Methods
        protected MetaModel()
        {
            Identity = new object();
        }

        /// <summary>
        /// Gets the ALinq.Mapping.MetaFunction that corresponds to a database function.
        /// </summary>
        /// <param name="method">The method defined on the ALinq.DataContext or subordinate class that represents the database function.</param>
        /// <returns>The meta-function that corresponds to a database function.</returns>
        public abstract MetaFunction GetFunction(MethodInfo method);

        /// <summary>
        /// Gets an enumeration of all functions.
        /// </summary>
        /// <returns>An enumeration that can be used to iterate through all functions.</returns>
        public abstract IEnumerable<MetaFunction> GetFunctions();

        /// <summary>
        /// Discovers the ALinq.Mapping.MetaType for the specified System.Type.
        /// </summary>
        /// <param name="type">The type for which the ALinq.Mapping.MetaType is sought.</param>
        /// <returns>A meta-type that corresponds to the specified type.</returns>
        public abstract MetaType GetMetaType(Type type);

        /// <summary>
        /// Gets the ALinq.Mapping.MetaTable associated with a specified System.Type.
        /// </summary>
        /// <param name="rowType">The common language runtime (CLR) row type.</param>
        /// <returns>A meta-table associated with the specified row type.</returns>
        public abstract MetaTable GetTable(Type rowType);

        /// <summary>
        /// Get an enumeration of all tables.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate over the tables.</returns>
        public abstract IEnumerable<MetaTable> GetTables();

        /// <summary>
        /// Gets the type of ALinq.DataContext type that this model describes.
        /// </summary>
        /// <returns>The data context type.</returns>
        public abstract Type ContextType { get; }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <returns>
        /// The database name as a string.
        /// </returns>
        public abstract string DatabaseName { get; }

        internal object Identity { get; private set; }

        /// <summary>
        /// Gets the mapping source that originated this model.
        /// </summary>
        /// <returns>The originating mapping source.</returns>
        public abstract MappingSource MappingSource { get; }

        /// <summary>
        /// Gets or sets the provider type.
        /// </summary>
        /// <returns>The provider type.</returns>
        public abstract Type ProviderType { get; }
    }




}
