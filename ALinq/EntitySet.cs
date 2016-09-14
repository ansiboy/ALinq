using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace ALinq
{
    /// <summary>
    /// Provides for deferred loading and relationship maintenance for the collection side of one-to-many and one-to-one relationships in a LINQ to SQL applications. 
    /// </summary>
    /// <typeparam name="TEntity">The data type of the target entity.</typeparam>
    public sealed class EntitySet<TEntity> : IList, IList<TEntity>, IListSource where TEntity : class
    {
        // Fields
        private IBindingList cachedList;
        private ItemList<TEntity> entities;
        private bool isLoaded;
        private bool isModified;
        private bool listChanged;
        private readonly Action<TEntity> onAdd;
        private TEntity onAddEntity;
        private readonly Action<TEntity> onRemove;
        private TEntity onRemoveEntity;
        private ItemList<TEntity> removedEntities;
        private IEnumerable<TEntity> source;
        private int version;

        /// <summary>
        /// Occurs when the contents of a list are changed.
        /// </summary>
        public event ListChangedEventHandler ListChanged;
       
 
        /// <summary>
        /// Initializes a new instance of the ALinq.EntitySet&lt;TEntity&gt; class.
        /// </summary>
        public EntitySet()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.EntitySet&lt;TEntity&gt; class while supplying handlers for add and remove operations.
        /// </summary>
        /// <param name="onAdd">Delegate for ALinq.EntitySet&lt;TEntity&gt;.Add(TEntity).</param>
        /// <param name="onRemove">Delegate for ALinq.EntitySet&lt;TEntity&gt;.Remove(TEntity).</param>
        public EntitySet(Action<TEntity> onAdd, Action<TEntity> onRemove)
        {
            this.onAdd = onAdd;
            this.onRemove = onRemove;
        }

        internal EntitySet(EntitySet<TEntity> es, bool copyNotifications)
        {
            this.source = es.source;
            ItemList<TEntity>.Enumerator enumerator = es.entities.GetEnumerator();
            while (enumerator.MoveNext())
            {
                TEntity current = enumerator.Current;
                this.entities.Add(current);
            }
            ItemList<TEntity>.Enumerator enumerator2 = es.removedEntities.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                TEntity item = enumerator2.Current;
                this.removedEntities.Add(item);
            }
            this.version = es.version;
            if (copyNotifications)
            {
                this.onAdd = es.onAdd;
                this.onRemove = es.onRemove;
            }
        }

        /// <summary>
        /// Adds an entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (entity != this.onAddEntity)
            {
                this.CheckModify();
                if (!this.entities.Contains(entity))
                {
                    this.OnAdd(entity);
                    if (this.HasSource)
                    {
                        this.removedEntities.Remove(entity);
                    }
                    this.entities.Add(entity);
                    if (this.IsLoaded)
                    {
                        this.OnListChanged(ListChangedType.ItemAdded, this.IndexOf(entity));
                    }
                }
                this.OnModified();
            }
        }

        /// <summary>
        /// Adds a collection of entities.
        /// </summary>
        /// <param name="collection">The collection to be added.</param>
        public void AddRange(IEnumerable<TEntity> collection)
        {
            if (collection == null)
            {
                throw Error.ArgumentNull("collection");
            }
            this.CheckModify();
            collection = collection.ToList<TEntity>();
            foreach (TEntity local in collection)
            {
                if (!this.entities.Contains(local))
                {
                    this.OnAdd(local);
                    if (this.HasSource)
                    {
                        this.removedEntities.Remove(local);
                    }
                    this.entities.Add(local);
                    if (this.IsLoaded)
                    {
                        this.OnListChanged(ListChangedType.ItemAdded, this.IndexOf(local));
                    }
                }
            }
            this.OnModified();
        }

        /// <summary>
        /// Assigns an ALinq.EntitySet&lt;TEntity&gt; collection to another ALinq.EntitySet&lt;TEntity&gt; collection.
        /// </summary>
        /// <param name="entitySource">The collection to assign.</param>
        public void Assign(IEnumerable<TEntity> entitySource)
        {
            this.Clear();
            if (entitySource != null)
            {
                this.AddRange(entitySource);
            }
            this.isLoaded = true;
        }

        private void CheckModify()
        {
            if ((this.onAddEntity != null) || (this.onRemoveEntity != null))
            {
                throw Error.ModifyDuringAddOrRemove();
            }
            this.version++;
        }

        /// <summary>
        /// Removes all items.
        /// </summary>
        public void Clear()
        {
            this.Load();
            this.CheckModify();
            if (this.entities.Items != null)
            {
                List<TEntity> list = new List<TEntity>(this.entities.Items);
                foreach (TEntity local in list)
                {
                    this.Remove(local);
                }
            }
            this.entities = new ItemList<TEntity>();
            this.OnModified();
            this.OnListChanged(ListChangedType.Reset, 0);
        }

        /// <summary>
        /// Specifies whether the ALinq.EntitySet&lt;TEntity&gt; contains a specific entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>true if the ALinq.EntitySet&lt;TEntity&gt; contains the entity; otherwise, false.</returns>
        public bool Contains(TEntity entity)
        {
            return (this.IndexOf(entity) >= 0);
        }

        /// <summary>
        /// Copies the ALinq.EntitySet&lt;TEntity&gt; to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The starting index in the array.</param>
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            this.Load();
            if (this.entities.Count > 0)
            {
                Array.Copy(this.entities.Items, 0, array, arrayIndex, this.entities.Count);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.Generic.IEnumerator&lt;T&gt;.</returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            this.Load();
            return new Enumerator((EntitySet<TEntity>)this);
        }

        /// <summary>
        /// Creates a new list for binding to a data source.
        /// </summary>
        /// <returns>A new System.ComponentModel.IBindingList for binding to a data source.</returns>
        public IBindingList GetNewBindingList()
        {
            return new EntitySetBindingList<TEntity>(this.ToList<TEntity>(), (EntitySet<TEntity>)this);
        }

        internal IEnumerable<TEntity> GetUnderlyingValues()
        {
            return new UnderlyingValues((EntitySet<TEntity>)this);
        }

        /// <summary>
        /// Returns the index of the entity.
        /// </summary>
        /// <param name="entity">The entity whose index is to be returned.</param>
        /// <returns>An integer representing the index.</returns>
        public int IndexOf(TEntity entity)
        {
            this.Load();
            return this.entities.IndexOf(entity);
        }

        /// <summary>
        /// Inserts an entity at an index position.
        /// </summary>
        /// <param name="index">The index representing the position at which to insert the entity.</param>
        /// <param name="entity">The entity to be inserted.</param>
        public void Insert(int index, TEntity entity)
        {
            this.Load();
            if ((index < 0) || (index > this.Count))
            {
                throw Error.ArgumentOutOfRange("index");
            }
            if ((entity == null) || (this.IndexOf(entity) >= 0))
            {
                throw Error.ArgumentOutOfRange("entity");
            }
            this.CheckModify();
            this.entities.Insert(index, entity);
            this.OnListChanged(ListChangedType.ItemAdded, index);
            this.OnAdd(entity);
        }

        /// <summary>
        /// Loads the ALinq.EntitySet&lt;TEntity&gt;.
        /// </summary>
        public void Load()
        {
            if (this.HasSource)
            {
                ItemList<TEntity> entities = this.entities;
                this.entities = new ItemList<TEntity>();
                foreach (TEntity local in this.source)
                {
                    this.entities.Add(local);
                }
                ItemList<TEntity>.Enumerator enumerator = entities.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    TEntity current = enumerator.Current;
                    this.entities.Include(current);
                }
                ItemList<TEntity>.Enumerator enumerator3 = this.removedEntities.GetEnumerator();
                while (enumerator3.MoveNext())
                {
                    TEntity item = enumerator3.Current;
                    this.entities.Remove(item);
                }
                this.source = SourceState<TEntity>.Loaded;
                this.isLoaded = true;
                this.removedEntities = new ItemList<TEntity>();
            }
        }

        private void OnAdd(TEntity entity)
        {
            if (this.onAdd != null)
            {
                TEntity onAddEntity = this.onAddEntity;
                this.onAddEntity = entity;
                try
                {
                    this.onAdd(entity);
                }
                finally
                {
                    this.onAddEntity = onAddEntity;
                }
            }
        }

        private void OnListChanged(ListChangedType type, int index)
        {
            this.listChanged = true;
            if (this.ListChanged != null)
            {
                this.ListChanged(this, new ListChangedEventArgs(type, index));
            }
        }

        private void OnModified()
        {
            this.isModified = true;
        }

        private void OnRemove(TEntity entity)
        {
            if (this.onRemove != null)
            {
                TEntity onRemoveEntity = this.onRemoveEntity;
                this.onRemoveEntity = entity;
                try
                {
                    this.onRemove(entity);
                }
                finally
                {
                    this.onRemoveEntity = onRemoveEntity;
                }
            }
        }

        /// <summary>
        /// Removes an entity.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        /// <returns>true if the entity is successfully removed; otherwise, false.</returns>
        public bool Remove(TEntity entity)
        {
            if ((entity == null) || (entity == this.onRemoveEntity))
            {
                return false;
            }
            this.CheckModify();
            int index = -1;
            bool flag = false;
            if (this.HasSource)
            {
                if (!this.removedEntities.Contains(entity))
                {
                    this.OnRemove(entity);
                    index = this.entities.IndexOf(entity);
                    if (index != -1)
                    {
                        this.entities.RemoveAt(index);
                    }
                    else
                    {
                        this.removedEntities.Add(entity);
                    }
                    flag = true;
                }
            }
            else
            {
                index = this.entities.IndexOf(entity);
                if (index != -1)
                {
                    this.OnRemove(entity);
                    this.entities.RemoveAt(index);
                    flag = true;
                }
            }
            if (flag)
            {
                this.OnModified();
                if (this.IsLoaded)
                {
                    this.OnListChanged(ListChangedType.ItemDeleted, index);
                }
            }
            return flag;
        }

        /// <summary>
        /// Removes an entity at a specified index.
        /// </summary>
        /// <param name="index">The index of the entity to be removed.</param>
        public void RemoveAt(int index)
        {
            this.Load();
            if ((index < 0) || (index >= this.Count))
            {
                throw Error.ArgumentOutOfRange("index");
            }
            this.CheckModify();
            TEntity entity = this.entities[index];
            this.OnRemove(entity);
            this.entities.RemoveAt(index);
            this.OnModified();
            this.OnListChanged(ListChangedType.ItemDeleted, index);
        }

        /// <summary>
        /// Sets the source of the ALinq.EntitySet&lt;TEntity&gt;.
        /// </summary>
        /// <param name="entitySource">The source of the ALinq.EntitySet&lt;TEntity&gt;.</param>
        public void SetSource(IEnumerable<TEntity> entitySource)
        {
            if (this.HasAssignedValues || this.HasLoadedValues)
            {
                throw Error.EntitySetAlreadyLoaded();
            }
            this.source = entitySource;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.Load();
            if (this.entities.Count > 0)
            {
                Array.Copy(this.entities.Items, 0, array, index, this.entities.Count);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int IList.Add(object value)
        {
            TEntity entity = value as TEntity;
            if ((entity == null) || (this.IndexOf(entity) >= 0))
            {
                throw Error.ArgumentOutOfRange("value");
            }
            this.CheckModify();
            int count = this.entities.Count;
            this.entities.Add(entity);
            this.OnAdd(entity);
            return count;
        }

        bool IList.Contains(object value)
        {
            return this.Contains(value as TEntity);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf(value as TEntity);
        }

        void IList.Insert(int index, object value)
        {
            TEntity entity = value as TEntity;
            if (value == null)
            {
                throw Error.ArgumentOutOfRange("value");
            }
            this.Insert(index, entity);
        }

        void IList.Remove(object value)
        {
            this.Remove(value as TEntity);
        }

        IList IListSource.GetList()
        {
            if ((this.cachedList == null) || this.listChanged)
            {
                this.cachedList = this.GetNewBindingList();
                this.listChanged = false;
            }
            return this.cachedList;
        }

        /// <summary>
        /// Gets the number of entities in the ALinq.EntitySet&lt;TEntity&gt; collection.
        /// </summary>
        /// <returns>
        /// An integer representing the number of entities.
        /// </returns>
        public int Count
        {
            get
            {
                this.Load();
                return this.entities.Count;
            }
        }

        internal bool HasAssignedValues
        {
            get
            {
                return this.isModified;
            }
        }

        /// <summary>
        /// Specifies whether the ALinq.EntitySet&lt;TEntity&gt; has loaded or assigned a value.
        /// </summary>
        /// <returns>
        /// Returns true if the ALinq.EntitySet&lt;TEntity&gt; has either loaded or assigned a value.
        /// </returns>
        public bool HasLoadedOrAssignedValues
        {
            get
            {
                if (!this.HasAssignedValues)
                {
                    return this.HasLoadedValues;
                }
                return true;
            }
        }

        internal bool HasLoadedValues
        {
            get
            {
                return this.isLoaded;
            }
        }

        internal bool HasSource
        {
            get
            {
                return ((this.source != null) && !this.HasLoadedValues);
            }
        }

        internal bool HasValues
        {
            get
            {
                if ((this.source != null) && !this.HasAssignedValues)
                {
                    return this.HasLoadedValues;
                }
                return true;
            }
        }

        /// <summary>
        /// Specifies whether this ALinq.EntitySet&lt;TEntity&gt; has a deferred query that has not yet executed.
        /// </summary>
        /// <returns>
        /// true if a deferred query has not yet been executed; otherwise false.
        /// </returns>
        public bool IsDeferred
        {
            get
            {
                return this.HasSource;
            }
        }

        internal bool IsLoaded
        {
            get
            {
                return this.isLoaded;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>An ALinq.EntitySet&lt;TEntity&gt; representing the item.</returns>
        public TEntity this[int index]
        {
            get
            {
                this.Load();
                if ((index < 0) || (index >= this.entities.Count))
                {
                    throw Error.ArgumentOutOfRange("index");
                }
                return this.entities[index];
            }
            set
            {
                this.Load();
                if ((index < 0) || (index >= this.entities.Count))
                {
                    throw Error.ArgumentOutOfRange("index");
                }
                if ((value == null) || (this.IndexOf(value) >= 0))
                {
                    throw Error.ArgumentOutOfRange("value");
                }
                this.CheckModify();
                TEntity entity = this.entities[index];
                this.OnRemove(entity);
                this.OnListChanged(ListChangedType.ItemDeleted, index);
                this.OnAdd(value);
                this.entities[index] = value;
                this.OnModified();
                this.OnListChanged(ListChangedType.ItemAdded, index);
            }
        }

        internal IEnumerable<TEntity> Source
        {
            get
            {
                return source;
            }
        }

        bool ICollection<TEntity>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                TEntity local = value as TEntity;
                if (value == null)
                {
                    throw Error.ArgumentOutOfRange("value");
                }
                this[index] = local;
            }
        }

        bool IListSource.ContainsListCollection
        {
            get
            {
                return true;
            }
        }

        // Nested Types
        private class Enumerable : IEnumerable<TEntity>
        {
            // Fields
            private readonly EntitySet<TEntity> entitySet;

            // Methods
            public Enumerable(EntitySet<TEntity> entitySet)
            {
                this.entitySet = entitySet;
            }

            public IEnumerator<TEntity> GetEnumerator()
            {
                return new Enumerator(entitySet);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private class Enumerator : IEnumerator<TEntity>
        {
            // Fields
            private readonly int endIndex;
            private readonly EntitySet<TEntity> entitySet;
            private int index;
            private readonly TEntity[] items;
            private readonly int version;

            // Methods
            public Enumerator(EntitySet<TEntity> entitySet)
            {
                this.entitySet = entitySet;
                this.items = entitySet.entities.Items;
                this.index = -1;
                this.endIndex = entitySet.entities.Count - 1;
                this.version = entitySet.version;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this.version != this.entitySet.version)
                {
                    throw Error.EntitySetModifiedDuringEnumeration();
                }
                if (this.index == this.endIndex)
                {
                    return false;
                }
                this.index++;
                return true;
            }

            void IEnumerator.Reset()
            {
                if (this.version != this.entitySet.version)
                {
                    throw Error.EntitySetModifiedDuringEnumeration();
                }
                this.index = -1;
            }

            // Properties
            public TEntity Current
            {
                get
                {
                    return this.items[this.index];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.items[this.index];
                }
            }
        }

        private class UnderlyingValues : IEnumerable<TEntity>
        {
            // Fields
            private readonly EntitySet<TEntity> entitySet;

            // Methods
            internal UnderlyingValues(EntitySet<TEntity> entitySet)
            {
                this.entitySet = entitySet;
            }

            public IEnumerator<TEntity> GetEnumerator()
            {
                return new Enumerator(entitySet);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }



}
