using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.Mapping;
using ALinq.SqlClient;
using ALinq.Provider;

namespace ALinq
{
#if DEBUG
    class MyMetaTables
    {
        private static MyMetaTables instance;
        internal readonly List<Type> tables;
        private static readonly object objLock = new object();
        private int count;

        private MyMetaTables()
        {
            tables = new List<Type>();
        }

        internal static MyMetaTables Instance
        {
            get
            {
                lock (objLock)
                {
                    if (instance == null)
                        instance = new MyMetaTables();
                }
                return instance;
            }
        }

        internal int Count
        {
            get
            {
                return count;
            }
        }

        public void Add(MetaTable table)
        {
            var type = table.RowType.Type;
            if (tables.Contains(type) == false)
            {
                lock (objLock)
                {
                    Debug.Print(type.ToString());
                    tables.Add(type);
                    count++;
                }
            }
        }
    }
#endif

    /// <summary>
    /// Represents a table for a particular type in the underlying database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the data in the table.</typeparam>
    public sealed partial class Table<TEntity> : IQueryable<TEntity>, IQueryProvider, ITable, ITable<TEntity>, IListSource where TEntity : class
    {
        // Fields
        private IBindingList cachedList;
        private readonly DataContext context;
        private readonly MetaTable metaTable;

        //private static List<MetaTable> tables;

        // Methods
        internal Table(DataContext context, MetaTable metaTable)
        {
            this.context = context;
            this.metaTable = metaTable;
#if TRIAL
            Debug.Assert(metaTable.Model != null);
            //foreach (var item in metaTable.Model.GetTables())
            MyMetaTables.Instance.Add(metaTable);

            if (MyMetaTables.Instance.Count > ALinqLicenseProvider.MAX_ENTITY_COUNT + 1)
            {
                throw Error.EntityMoreThan18(metaTable.Model.ProviderType, context.Provider);
            }
#endif
        }



#if DEBUG
        internal int GetMetaTablesCount()
        {
            return MyMetaTables.Instance.Count;
        }
#endif

        /// <summary>
        /// Attaches a disconnected or "detached" entity to a new ALinq.DataContext when original values are required for optimistic concurrency checks.
        /// </summary>
        /// <param name="entity"></param>
        public void Attach(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            Attach(entity, false);
        }


#if DEBUG
        //Detach ипн╢й╣ож
        public void Detach(TEntity entity)
        {
            //TODO:Detach
            this.context.Services.ChangeTracker.StopTracking(entity);
        }
#endif

        /// <summary>
        /// Attaches an entity to the ALinq.DataContext in either a modified or unmodified state.
        /// </summary>
        /// <param name="entity">The entity to be attached.</param>
        /// <param name="asModified">True to attach the entity as modified.</param>
        public void Attach(TEntity entity, bool asModified)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            MetaType inheritanceType = metaTable.RowType.GetInheritanceType(entity.GetType());
            if (!IsTrackableType(inheritanceType))
            {
                throw Error.TypeCouldNotBeTracked(inheritanceType.Type);
            }
            if (asModified && ((inheritanceType.VersionMember == null) && inheritanceType.HasUpdateCheck))
            {
                throw Error.CannotAttachAsModifiedWithoutOriginalState();
            }
            TrackedObject trackedObject = Context.Services.ChangeTracker.GetTrackedObject(entity);
            if ((trackedObject != null) && !trackedObject.IsWeaklyTracked)
            {
                throw Error.CannotAttachAlreadyExistingEntity();
            }
            if (trackedObject == null)
            {
                trackedObject = context.Services.ChangeTracker.Track(entity, true);
            }
            if (asModified)
            {
                trackedObject.ConvertToModified();
            }
            else
            {
                trackedObject.ConvertToUnmodified();
            }
            if (Context.Services.InsertLookupCachedObject(inheritanceType, entity) != entity)
            {
                throw new DuplicateKeyException(entity, Strings.CantAddAlreadyExistingKey);
            }
            trackedObject.InitializeDeferredLoaders();

            //TODO:SetDataContext
            if (metaTable.RowType.HasAnySetDataContextMethod)
                context.Services.SendSetDataContext(inheritanceType, entity, new object[] { this.context });
        }

        internal enum UpdateType
        {
            Insert,
            Update,
            Delete
        }

        public int Insert(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            //CheckReadOnly();
            //context.CheckNotInSubmitChanges();
            var metaType = Context.Mapping.GetMetaType(typeof(TEntity));
            var autoSyncMembers = GetAutoSyncMembers(metaTable.RowType, UpdateType.Insert);
            var source = Expression.Parameter(metaTable.RowType.Type, "p");
            Expression insertCommand;
            if (autoSyncMembers.Count > 0)
            {
                var resultSelector = Expression.Lambda(CreateAutoSync(autoSyncMembers, source), new[] { source });
                var argTypes = new[] { metaType.InheritanceRoot.Type, resultSelector.Body.Type };
                var args = new Expression[] { Expression.Constant(entity), resultSelector };
                insertCommand = Expression.Call(typeof(DataManipulation), "Insert", argTypes, args);
            }
            else
            {
                var argTypes = new[] { metaType.InheritanceRoot.Type };
                var args = new Expression[] { Expression.Constant(entity) };
                insertCommand = Expression.Call(typeof(DataManipulation), "Insert", argTypes, args);
            }

            if (insertCommand.Type == typeof(int))
            {
                return (int)context.Provider.Execute(insertCommand).ReturnValue;
            }
            var result = context.Provider.Execute(insertCommand).ReturnValue;
            var returnValue = (IEnumerable<object>)result;
            object[] syncResults;
            var value = returnValue.FirstOrDefault();

            if (value is object[])
                syncResults = (object[])value;
            else
                syncResults = new[] { value };

            if (syncResults == null)
            {
                throw Error.InsertAutoSyncFailure();
            }
            AutoSyncMembers(syncResults, entity, UpdateType.Insert);
            return 1;
        }

        internal static Expression CreateAutoSync(ICollection<MetaDataMember> membersToSync, Expression source)
        {
            int num = 0;
            var initializers = new Expression[membersToSync.Count];
            foreach (var member in membersToSync)
            {
                initializers[num++] = Expression.Convert(GetMemberExpression(source, member.Member), typeof(object));
            }
            return Expression.NewArrayInit(typeof(object), initializers);
        }

        private static Expression GetMemberExpression(Expression exp, MemberInfo mi)
        {
            var field = mi as FieldInfo;
            if (field != null)
            {
                return Expression.Field(exp, field);
            }
            var property = (PropertyInfo)mi;
            return Expression.Property(exp, property);
        }

        internal static List<MetaDataMember> GetAutoSyncMembers(MetaType metaType, UpdateType updateType)
        {
            var list = new List<MetaDataMember>();
            foreach (MetaDataMember member in metaType.PersistentDataMembers.OrderBy(m => m.Ordinal))
            {
                if ((((updateType != UpdateType.Insert) || (member.AutoSync != AutoSync.OnInsert)) &&
                     ((updateType != UpdateType.Update) || (member.AutoSync != AutoSync.OnUpdate))) &&
                    (member.AutoSync != AutoSync.Always))
                {
                    continue;
                }
                list.Add(member);
            }
            return list;
        }

        internal void AutoSyncMembers(object[] syncResults, TEntity item, UpdateType updateType)
        {
            if (syncResults != null)
            {
                var metaType = Context.Mapping.GetMetaType(typeof(TEntity));
                int num = 0;
                var members = GetAutoSyncMembers(metaType, updateType);
                foreach (MetaDataMember member in members)
                {
                    object obj2 = syncResults[num++];
                    object current = item;
                    if ((member.Member is PropertyInfo) && ((PropertyInfo)member.Member).CanWrite)
                    {
                        member.MemberAccessor.SetBoxedValue(ref current, DBConvert.ChangeType(obj2, member.Type));
                    }
                    else
                    {
                        member.StorageAccessor.SetBoxedValue(ref current, DBConvert.ChangeType(obj2, member.Type));
                    }
                }
            }
        }





        // Func<TSource, TResult> selector


        /// <summary>
        /// Attaches an entity to the ALinq.DataContext in either a modified or unmodified state by specifying both the entity and its original state.
        /// </summary>
        /// <param name="entity">The entity to be attached.</param>
        /// <param name="original">An instance of the same entity type with data members that contain the original values.</param>
        public void Attach(TEntity entity, TEntity original)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (original == null)
            {
                throw Error.ArgumentNull("original");
            }
            if (entity.GetType() != original.GetType())
            {
                throw Error.OriginalEntityIsWrongType();
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            MetaType inheritanceType = metaTable.RowType.GetInheritanceType(entity.GetType());
            if (!IsTrackableType(inheritanceType))
            {
                throw Error.TypeCouldNotBeTracked(inheritanceType.Type);
            }
            TrackedObject trackedObject = context.Services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject != null && !trackedObject.IsWeaklyTracked)
            {
                throw Error.CannotAttachAlreadyExistingEntity();
            }
            if (trackedObject == null)
            {
                trackedObject = context.Services.ChangeTracker.Track(entity, true);
            }
            trackedObject.ConvertToPossiblyModified(original);
            if (Context.Services.InsertLookupCachedObject(inheritanceType, entity) != entity)
            {
                throw new DuplicateKeyException(entity, Strings.CantAddAlreadyExistingKey);
            }
            trackedObject.InitializeDeferredLoaders();
        }

        /// <summary>
        /// Attaches all entities of a collection to the ALinq.DataContext in either a modified or unmodified state.
        /// </summary>
        /// <typeparam name="TSubEntity">The type of entities to attach.</typeparam>
        /// <param name="entities">The collection of entities.</param>
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            AttachAll(entities, false);
        }

        /// <summary>
        /// Attaches all entities of a collection to the ALinq.DataContext in either a modified or unmodified state.
        /// </summary>
        /// <typeparam name="TSubEntity">The type of entities to attach.</typeparam>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="asModified">true if the object has a timestamp or RowVersion member, false if original values are being used for the optimistic concurrency check.</param>
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities, bool asModified) where TSubEntity : TEntity
        {
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            foreach (TSubEntity local in entities.ToList())
            {
                Attach(local, asModified);
            }
        }

        private void CheckReadOnly()
        {
            if (IsReadOnly)
            {
                throw Error.CannotPerformCUDOnReadOnlyTable(ToString());
            }
        }

        /// <summary>
        /// Puts all entities from the collection into a pending delete state.
        /// </summary>
        /// <typeparam name="TSubEntity">The type of the elements to delete.</typeparam>
        /// <param name="entities">The entities being removed.</param>
        public void DeleteAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            foreach (var local in entities.ToList())
            {
                DeleteOnSubmit(local);
            }
        }

        /// <summary>
        /// Puts an entity from this table into a pending delete state.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        public void DeleteOnSubmit(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            TrackedObject trackedObject = context.Services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject == null)
            {
                throw Error.CannotRemoveUnattachedEntity();
            }
            if (trackedObject.IsNew)
            {
                trackedObject.ConvertToRemoved();
            }
            else if (trackedObject.IsPossiblyModified || trackedObject.IsModified)
            {
                trackedObject.ConvertToDeleted();
            }
        }

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return
                ((IEnumerable<TEntity>)context.Provider.Execute(Expression.Constant(this)).ReturnValue).
                    GetEnumerator();
        }

        /// <summary>
        /// Returns an array of modified members that contain their current and original values.
        /// </summary>
        /// <param name="entity">The entity from which to get the array.</param>
        /// <returns>An array of modified members.</returns>
        public ModifiedMemberInfo[] GetModifiedMembers(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            MetaType metaType = Context.Mapping.GetMetaType(entity.GetType());
            if ((metaType == null) || !metaType.IsEntity)
            {
                throw Error.EntityIsTheWrongType();
            }
            TrackedObject trackedObject = Context.Services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject != null)
            {
                return trackedObject.GetModifiedMembers().ToArray();
            }
            return new ModifiedMemberInfo[0];
        }

        /// <summary>
        /// Creates a new list for binding to a data source.
        /// </summary>
        /// <returns>A new System.ComponentModel.IBindingList for binding to a data source.</returns>
        public IBindingList GetNewBindingList()
        {
            return BindingList.Create(context, this);
        }

        /// <summary>
        /// Returns an instance that contains the original state of the entity.
        /// </summary>
        /// <param name="entity">The entity whose original state is to be returned.</param>
        /// <returns>A ALinq.Table&lt;TEntity&gt; instance in its original state.</returns>
        public TEntity GetOriginalEntityState(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            MetaType metaType = Context.Mapping.GetMetaType(entity.GetType());
            if ((metaType == null) || !metaType.IsEntity)
            {
                throw Error.EntityIsTheWrongType();
            }
            TrackedObject trackedObject = Context.Services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject == null)
            {
                return default(TEntity);
            }
            if (trackedObject.Original != null)
            {
                return (TEntity)trackedObject.CreateDataCopy(trackedObject.Original);
            }
            return (TEntity)trackedObject.CreateDataCopy(trackedObject.Current);
        }

        /// <summary>
        /// Adds all entities of a collection to the ALinq.DataContext in a pending insert state.
        /// </summary>
        /// <typeparam name="TSubEntity">The type of the elements to insert.</typeparam>
        /// <param name="entities">The entities to add.</param>
        public void InsertAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            foreach (var local in entities.ToList())
            {
                InsertOnSubmit(local);
            }
        }

        /// <summary>
        /// Adds an entity in a pending insert state to this ALinq.Table<TEntity>.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        public void InsertOnSubmit(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            MetaType inheritanceType = metaTable.RowType.GetInheritanceType(entity.GetType());
            if (!IsTrackableType(inheritanceType))
            {
                throw Error.TypeCouldNotBeAdded(inheritanceType.Type);
            }
            TrackedObject trackedObject = context.Services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject == null)
            {
                context.Services.ChangeTracker.Track(entity).ConvertToNew();
            }
            else if (trackedObject.IsWeaklyTracked)
            {
                trackedObject.ConvertToNew();
            }
            else if (trackedObject.IsDeleted)
            {
                trackedObject.ConvertToPossiblyModified();
            }
            else if (trackedObject.IsRemoved)
            {
                trackedObject.ConvertToNew();
            }
            else if (!trackedObject.IsNew)
            {
                throw Error.CantAddAlreadyExistingItem();
            }

        }

        private static bool IsTrackableType(MetaType type)
        {
            if (type == null)
            {
                return false;
            }
            if (!type.CanInstantiate)
            {
                return false;
            }
            if (type.HasInheritance && !type.HasInheritanceCode)
            {
                return false;
            }
            return true;
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IList IListSource.GetList()
        {
            if (cachedList == null)
            {
                cachedList = GetNewBindingList();
            }
            return cachedList;
        }

        void ITable.Attach(object entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            var local = entity as TEntity;
            if (local == null)
            {
                throw Error.EntityIsTheWrongType();
            }
            Attach(local, false);
        }

        void ITable.Attach(object entity, bool asModified)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            var local = entity as TEntity;
            if (local == null)
            {
                throw Error.EntityIsTheWrongType();
            }
            Attach(local, asModified);
        }

        void ITable.Attach(object entity, object original)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (original == null)
            {
                throw Error.ArgumentNull("original");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            var local = entity as TEntity;
            if (local == null)
            {
                throw Error.EntityIsTheWrongType();
            }
            if (entity.GetType() != original.GetType())
            {
                throw Error.OriginalEntityIsWrongType();
            }
            Attach(local, (TEntity)original);
        }

        void ITable.AttachAll(IEnumerable entities)
        {
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            ((ITable)this).AttachAll(entities, false);
        }

        void ITable.AttachAll(IEnumerable entities, bool asModified)
        {
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            List<object> list = entities.Cast<object>().ToList();
            ITable table = this;
            foreach (object obj2 in list)
            {
                table.Attach(obj2, asModified);
            }
        }

        void ITable.DeleteAllOnSubmit(IEnumerable entities)
        {
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            List<object> list = entities.Cast<object>().ToList();
            ITable table = this;
            foreach (object obj2 in list)
            {
                table.DeleteOnSubmit(obj2);
            }
        }

        void ITable.DeleteOnSubmit(object entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            var local = entity as TEntity;
            if (local == null)
            {
                throw Error.EntityIsTheWrongType();
            }
            DeleteOnSubmit(local);
        }

        ModifiedMemberInfo[] ITable.GetModifiedMembers(object entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            var local = entity as TEntity;
            if (local == null)
            {
                throw Error.EntityIsTheWrongType();
            }
            return GetModifiedMembers(local);
        }

        object ITable.GetOriginalEntityState(object entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            var local = entity as TEntity;
            if (local == null)
            {
                throw Error.EntityIsTheWrongType();
            }
            return GetOriginalEntityState(local);
        }

        void ITable.InsertAllOnSubmit(IEnumerable entities)
        {
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            List<object> list = entities.Cast<object>().ToList();
            ITable table = this;
            foreach (object obj2 in list)
            {
                table.InsertOnSubmit(obj2);
            }
        }

        void ITable.InsertOnSubmit(object entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            var local = entity as TEntity;
            if (local == null)
            {
                throw Error.EntityIsTheWrongType();
            }
            InsertOnSubmit(local);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull("expression");
            }
            Type elementType = TypeSystem.GetElementType(expression.Type);
            Type type2 = typeof(IQueryable<>).MakeGenericType(new[] { elementType });
            if (!type2.IsAssignableFrom(expression.Type))
            {
                throw Error.ExpectedQueryableArgument("expression", type2);
            }
            return
                (IQueryable)
                Activator.CreateInstance(typeof(DataQuery<>).MakeGenericType(new[] { elementType }),
                                         new object[] { context, expression });
        }

        IQueryable<TResult> IQueryProvider.CreateQuery<TResult>(Expression expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull("expression");
            }
            if (!typeof(IQueryable<TResult>).IsAssignableFrom(expression.Type))
            {
                throw Error.ExpectedQueryableArgument("expression", typeof(IEnumerable<TResult>));
            }
            return new DataQuery<TResult>(context, expression);
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            return (TResult)context.Provider.Execute(expression).ReturnValue;
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return context.Provider.Execute(expression).ReturnValue;
        }

        /// <summary>
        /// Returns a string that represents the table.
        /// </summary>
        /// <returns>A string representing the table.</returns>
        public override string ToString()
        {
            return ("Table(" + typeof(TEntity).Name + ")");
        }

        /// <summary>
        /// Gets the ALinq.DataContext that has been used to retrieve this ALinq.Table<TEntity>.
        /// </summary>
        /// <returns>
        /// The ALinq.DataContext.
        /// </returns>
        public DataContext Context
        {
            get { return context; }
        }

#if DEBUG
        public MetaTable MetaTable
        {
            get { return this.metaTable; }
        }
#endif

        /// <summary>
        /// Gets a value that indicates the value of ALinq.DataContext.ObjectTrackingEnabled in the ALinq.DataContext that has been used to retrieved this ALinq.Table<TEntity>.
        /// </summary>
        /// <returns>
        /// Returns true if ALinq.DataContext.ObjectTrackingEnabled is false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return !metaTable.RowType.IsEntity; }
        }

        bool IListSource.ContainsListCollection
        {
            [DebuggerStepThrough]
            get { return false; }
        }

        Type IQueryable.ElementType
        {
            [DebuggerStepThrough]
            get { return typeof(TEntity); }
        }

        Expression IQueryable.Expression
        {
            [DebuggerStepThrough]
            get
            {
                var result = Expression.Constant(this);
                return result;
            }
        }

        IQueryProvider IQueryable.Provider
        {
            [DebuggerStepThrough]
            get { return this; }
        }
    }
}