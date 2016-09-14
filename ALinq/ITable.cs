using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using ALinq;
using System.Linq;
using ALinq.Mapping;

namespace ALinq
{
    /// <summary>
    /// Used for weakly typed query scenarios.
    /// </summary>
    public interface ITable : IQueryable
    {
        /// <summary>
        /// Attaches an entity to the ALinq.DataContext in an unmodified state.
        /// </summary>
        /// <param name="entity">The entity to be attached.</param>
        void Attach(object entity);
        
        /// <summary>
        /// Attaches all entities of a collection to the ALinq.DataContext in either a modified or unmodified state.
        /// </summary>
        /// <param name="entity">The collection of entities.</param>
        /// <param name="asModified">true to attach the entities as modified.</param>
        void Attach(object entity, bool asModified);
       
        /// <summary>
        /// Attaches an entity to the ALinq.DataContext in either a modified or unmodified state by specifying both the entity and its original state.
        /// </summary>
        /// <param name="entity">The entity to be attached.</param>
        /// <param name="original">An instance of the same entity type with data members that contain the original values.</param>
        void Attach(object entity, object original);

        /// <summary>
        /// Attaches all entities of a collection to the ALinq.DataContext in either a modified or unmodified state.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        void AttachAll(IEnumerable entities);

        /// <summary>
        /// Attaches all entities of a collection to the ALinq.DataContext in either a modified or unmodified state.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="asModified">true to attach the entities as modified.</param>
        void AttachAll(IEnumerable entities, bool asModified);

        /// <summary>
        /// Puts all entities from the collection into a pending delete state.
        /// </summary>
        /// <param name="entities">The collection from which all items are removed.</param>
        void DeleteAllOnSubmit(IEnumerable entities);

        /// <summary>
        /// Puts an entity from this table into a pending delete state.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        void DeleteOnSubmit(object entity);

        /// <summary>
        /// Returns an array of modified members that contain their current and original values.
        /// </summary>
        /// <param name="entity">The entity from which to get the array.</param>
        ModifiedMemberInfo[] GetModifiedMembers(object entity);

        /// <summary>
        /// Retrieves original values.
        /// </summary>
        /// <param name="entity">The entity whose original value is to be retrieved.</param>
        /// <returns>A copy of the original entity. The value is null if the entity passed in is not tracked. Disconnected entities sent back by a client must be attached before the ALinq.DataContext can begin to track their state. The "original state" of a newly attached entity is established based on values supplied by the client. The data context does not track the state of disconnected entities.</returns>
        object GetOriginalEntityState(object entity);

        /// <summary>
        /// Adds all entities of a collection to the ALinq.DataContext in a pending insert state.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        void InsertAllOnSubmit(IEnumerable entities);

        /// <summary>
        /// Adds an entity in a pending insert state to this table.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        void InsertOnSubmit(object entity);

       /// <summary>
        /// Gets the ALinq.DataContext that has been used to retrieve this ALinq.ITable.
       /// </summary>
        DataContext Context { get; }

        /// <summary>
        /// Returns true if ALinq.DataContext.ObjectTrackingEnabled is false in the ALinq.DataContext that has been used to retrieve this ALinq.ITable.
        /// </summary>
        bool IsReadOnly { get; }
        //int Update(Type TEntity, Type TResult, Expression entity, Expression predicate);
        //int Delete(Type TEntity, Expression predicate);
#if DEBUG
        MetaTable MetaTable { get; }
#endif


    }

    /// <summary>Represents a table for a particular type in the underlying database. </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ITable<TEntity> : IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable where TEntity : class
    {
        /// <summary>When overridden, adds an entity in a pending insert state to this ITable&lt;TEntity&gt;.</summary>
        /// <param name="entity">The object to insert.</param>
        void InsertOnSubmit(TEntity entity);
        /// <summary>When overridden, attaches a disconnected or "detached" entity to a new DataContext when original values are required for optimistic concurrency checks.</summary>
        /// <param name="entity">The object to be added.</param>
        void Attach(TEntity entity);
        /// <summary>When overridden, puts an entity from this table into a pending delete state.</summary>
        /// <param name="entity">The object to delete.</param>
        void DeleteOnSubmit(TEntity entity);
    }
}