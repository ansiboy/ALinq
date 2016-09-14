using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ALinq
{
    /// <summary>
    /// Provides for deferred loading and relationship maintenance for the singleton side of a one-to-many relationship in a LINQ to SQL application.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityRef<TEntity> where TEntity : class
    {
        private IEnumerable<TEntity> source;
        private TEntity entity;
       
        /// <summary>
        /// Initializes a new instance of the ALinq.EntityRef&lt;TEntity&gt; class by referencing the target entity.
        /// </summary>
        /// <param name="entity">The target entity.</param>
        public EntityRef(TEntity entity)
        {
            this.entity = entity;
            this.source = SourceState<TEntity>.Assigned;
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.EntityRef&lt;TEntity&gt; class by specifying the source.
        /// </summary>
        /// <param name="source">The reference source.</param>
        public EntityRef(IEnumerable<TEntity> source)
        {
            this.source = source;
            this.entity = default(TEntity);
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.EntityRef&lt;TEntity&gt; class by referencing the target entity.
        /// </summary>
        /// <param name="entityRef">The target entity.</param>
        public EntityRef(EntityRef<TEntity> entityRef)
        {
            this.source = entityRef.source;
            this.entity = entityRef.entity;
        }

        /// <summary>
        /// Gets or sets the target entity.
        /// </summary>
        /// <returns>
        /// The target entity.
        /// </returns>
        public TEntity Entity
        {
            get
            {
                if (this.HasSource)
                {
                    this.entity = this.source.SingleOrDefault<TEntity>();
                    this.source = SourceState<TEntity>.Loaded;
                }
                return this.entity;
            }
            set
            {
                this.entity = value;
                this.source = SourceState<TEntity>.Assigned;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the target has been loaded or assigned.
        /// </summary>
        /// <returns>
        /// True if the target has been loaded or assigned.
        /// </returns>
        public bool HasLoadedOrAssignedValue
        {
            get
            {
                if (!this.HasLoadedValue)
                {
                    return this.HasAssignedValue;
                }
                return true;
            }
        }

        internal bool HasValue
        {
            get
            {
                if ((this.source != null) && !this.HasLoadedValue)
                {
                    return this.HasAssignedValue;
                }
                return true;
            }
        }

        internal bool HasLoadedValue
        {
            get
            {
                return (this.source == SourceState<TEntity>.Loaded);
            }
        }

        internal bool HasAssignedValue
        {
            get
            {
                return (this.source == SourceState<TEntity>.Assigned);
            }
        }

        internal bool HasSource
        {
            get
            {
                return (((this.source != null) && !this.HasLoadedValue) && !this.HasAssignedValue);
            }
        }

        internal IEnumerable<TEntity> Source
        {
            get
            {
                return this.source;
            }
        }

        internal TEntity UnderlyingValue
        {
            get
            {
                return this.entity;
            }
        }
    }
}