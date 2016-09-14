using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ALinq
{
    /// <summary>
    /// Used to enable deferred loading of individual properties (similar to ALinq.EntityRef&lt;TEntity&gt;).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the deferred source.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public struct Link<T>
    {
        private T underlyingValue;
        private IEnumerable<T> source;

        /// <summary>
        /// Initializes a new instance of the ALinq.Link&lt;T&gt; structure by referencing the value of the property.
        /// </summary>
        /// <param name="value">The value for the property.</param>
        public Link(T value)
        {
            this.underlyingValue = value;
            this.source = null;
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.Link&lt;T&gt; structure by referencing the source.
        /// </summary>
        /// <param name="source">The source collection.</param>
        public Link(IEnumerable<T> source)
        {
            this.source = source;
            this.underlyingValue = default(T);
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.Link&lt;T&gt; structure by copying the internal state from another ALinq.Link&lt;T&gt; instance.
        /// </summary>
        /// <param name="link">The ALinq.Link&lt;T&gt; instance from which to copy.</param>
        public Link(Link<T> link)
        {
            this.underlyingValue = link.underlyingValue;
            this.source = link.source;
        }

        /// <summary>
        /// Gets a value that specifies whether the source has a value.
        /// </summary>
        /// <returns>
        /// Returns true if the source has an assigned or loaded value (including null).
        /// </returns>
        public bool HasValue
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

        /// <summary>
        /// Specifies whether the ALinq.Link&lt;T&gt; has loaded or assigned a value.
        /// </summary>
        /// <returns>
        /// true if the ALinq.Link&lt;T&gt; has either loaded or assigned a value; otherwise false.
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
        internal bool HasLoadedValue
        {
            get
            {
                return (this.source == SourceState<T>.Loaded);
            }
        }
        internal bool HasAssignedValue
        {
            get
            {
                return (this.source == SourceState<T>.Assigned);
            }
        }
        internal T UnderlyingValue
        {
            get
            {
                return this.underlyingValue;
            }
        }
        internal IEnumerable<T> Source
        {
            get
            {
                return this.source;
            }
        }
        internal bool HasSource
        {
            get
            {
                return (((this.source != null) && !this.HasAssignedValue) && !this.HasLoadedValue);
            }
        }

        /// <summary>
        /// Gets or sets the value assigned to or loaded by the ALinq.Link&lt;T&gt;.
        /// </summary>
        /// <returns>
        /// The value of this deferred property.
        /// </returns>
        public T Value
        {
            get
            {
                if (this.HasSource)
                {
                    this.underlyingValue = this.source.SingleOrDefault<T>();
                    this.source = SourceState<T>.Loaded;
                }
                return this.underlyingValue;
            }
            set
            {
                this.underlyingValue = value;
                this.source = SourceState<T>.Assigned;
            }
        }
    }
}