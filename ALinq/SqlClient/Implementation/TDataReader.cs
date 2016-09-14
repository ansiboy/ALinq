using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.SqlClient.Implementation
{
    /// <summary>
    /// Defines methods for dynamically materializing objects.
    /// </summary>
    /// <typeparam name="TDataReader">The type of the data reader.</typeparam>
    public abstract class ObjectMaterializer<TDataReader> where TDataReader : DbDataReader
    {
        /// <summary>
        /// Captures internal state for the fast materializer.
        /// </summary>
        public object[] Arguments;

        /// <summary>
        /// Represents a reader that reads data rows in a forward-only manner.
        /// </summary>
        public DbDataReader BufferReader;

        /// <summary>
        /// Represents a data reader.
        /// </summary>
        public TDataReader DataReader;

        /// <summary>
        /// Captures internal state for the fast materializer.
        /// </summary>
        public object[] Globals;

        /// <summary>
        /// Captures internal state for the fast materializer.
        /// </summary>
        public object[] Locals;

        /// <summary>
        /// Represents column ordinals of a data reader.
        /// </summary>
        public int[] Ordinals;

        /// <summary>
        /// Initializes a new instance of the ALinq.SqlClient.Implementation.ObjectMaterializer&lt;TDataReader&gt; class.
        /// </summary>
        protected ObjectMaterializer()
        {
            this.DataReader = default(TDataReader);
        }

        /// <summary>
        /// Changes the type of each element in a specified sequence.
        /// </summary>
        /// <typeparam name="TOutput">The type to convert the elements to.</typeparam>
        /// <param name="source">A sequence that contains elements to convert.</param>
        /// <returns>A sequence that contains the type-converted elements.</returns>
        public static IEnumerable<TOutput> Convert<TOutput>(IEnumerable source)
        {
            //<Convert>d__0<TDataReader, TOutput> d__ = new <Convert>d__0<TDataReader, TOutput>(-2);
            //d__.<>3__source = source;
            //return d__;
            foreach (var item in source)
                yield return DBConvert.ChangeType<TOutput>(item);
        }

        /// <summary>
        /// Creates a group from a specified key and collection of values.
        /// </summary>
        /// <typeparam name="TKey">The type of the key of the group.</typeparam>
        /// <typeparam name="TElement">The type of the values in the group.</typeparam>
        /// <param name="key">The key for the group.</param>
        /// <param name="items">The values for the group.</param>
        /// <returns>A group that has the specified key and the specified collection of values.</returns>
        public static IGrouping<TKey, TElement> CreateGroup<TKey, TElement>(TKey key, IEnumerable<TElement> items)
        {
            return new ObjectReaderCompiler.Group<TKey, TElement>(key, items);
        }

        /// <summary>
        /// Creates an ordered sequence from a specified collection of values.
        /// </summary>
        /// <typeparam name="TElement">The type of the values in the ordered sequence.</typeparam>
        /// <param name="items">The values to put in the ordered sequence.</param>
        /// <returns>An ordered sequence that contains the specified values.</returns>
        public static IOrderedEnumerable<TElement> CreateOrderedEnumerable<TElement>(IEnumerable<TElement> items)
        {
            return new ObjectReaderCompiler.OrderedResults<TElement>(items);
        }

        /// <summary>
        /// Returns an exception that indicates that a null value was tried to be assigned to a non-nullable value type.
        /// </summary>
        /// <param name="type">The type to which a null value was attempted to be assigned.</param>
        /// <returns>An exception that indicates that a null value was attempted to be assigned to a non-nullable value type.</returns>
        public static Exception ErrorAssignmentToNull(Type type)
        {
            return SqlClient.Error.CannotAssignNull(type);
        }

        /// <summary>
        /// When overridden in a derived class, executes a query.
        /// </summary>
        /// <param name="iSubQuery">The index of the query.</param>
        /// <param name="args">The arguments to the query.</param>
        /// <returns>The results from executing the query.</returns>
        public abstract IEnumerable ExecuteSubQuery(int iSubQuery, object[] args);

        /// <summary>
        /// When overridden in a derived class, creates a new deferred source.
        /// </summary>
        /// <typeparam name="T">The type of the result elements.</typeparam>
        /// <param name="globalLink">The index of the link.</param>
        /// <param name="localFactory">The index of the factory.</param>
        /// <param name="keyValues">The key values for the deferred source.</param>
        /// <returns>An enumerable deferred source.</returns>
        public abstract IEnumerable<T> GetLinkSource<T>(int globalLink, int localFactory, object[] keyValues);

        /// <summary>
        /// When overridden in a derived class, creates a new deferred source.
        /// </summary>
        /// <typeparam name="T">The type of the result elements.</typeparam>
        /// <param name="globalLink">The index of the link.</param>
        /// <param name="localFactory">The index of the factory.</param>
        /// <param name="instance">The instance for the deferred source.</param>
        /// <returns>An enumerable deferred source.</returns>
        public abstract IEnumerable<T> GetNestedLinkSource<T>(int globalLink, int localFactory, object instance);
        
        /// <summary>
        /// When overridden in a derived class, inserts a value into a data structure.
        /// </summary>
        /// <param name="globalMetaType">The index of the ALinq.Mapping.MetaType.</param>
        /// <param name="instance">The object to insert into the data structure.</param>
        /// <returns>The value that was inserted into the data structure.</returns>
        public abstract object InsertLookup(int globalMetaType, object instance);

        /// <summary>
        /// When overridden in a derived class, advances the reader to the next record.
        /// </summary>
        /// <returns>true if there are more rows; otherwise, false.</returns>
        public abstract bool Read();

        /// <summary>
        /// When overridden in a derived class, invokes the method represented by ALinq.Mapping.MetaType.OnLoadedMethod.
        /// </summary>
        /// <param name="globalMetaType">The index of the ALinq.Mapping.MetaType.</param>
        /// <param name="instance">The parameter to pass to the invoked method.</param>
        public abstract void SendEntityMaterialized(int globalMetaType, object instance);

        /// <summary>
        /// When overridden in a derived class, gets a value that indicates whether deferred loading is enabled.
        /// </summary>
        /// <returns>
        /// true if deferred loading is enabled; otherwise, false.
        /// </returns>
        public abstract bool CanDeferLoad { get; }


    }
}