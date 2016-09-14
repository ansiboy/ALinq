using System;

namespace ALinq.Mapping
{
    /// <summary>
    /// Represents an accessor to a member.
    /// </summary>
    public abstract class MetaAccessor
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.MetaAccessor class.
        /// </summary>
        protected MetaAccessor()
        {
        }

        /// <summary>
        /// Specifies an object on which to set a value or from which to get a value.
        /// </summary>
        /// <param name="instance">The instance from which to get the value or on which to set the value.</param>
        /// <returns>The boxed value of this instance.</returns>
        public abstract object GetBoxedValue(object instance);

        /// <summary>
        /// Specifies whether the instance has an assigned value.
        /// </summary>
        /// <param name="instance">The instance being looked at.</param>
        /// <returns>true if the instance currently has an assigned value; otherwise, false.</returns>
        public virtual bool HasAssignedValue(object instance)
        {
            return true;
        }

        /// <summary>
        /// Specifies whether the instance has a value loaded from a deferred source.
        /// </summary>
        /// <param name="instance">The instance being looked at.</param>
        /// <returns>true if the instance currently has a value loaded from a deferred source; otherwise, false.</returns>
        public virtual bool HasLoadedValue(object instance)
        {
            return false;
        }

        /// <summary>
        /// Specifies whether the instance has a loaded or assigned value.
        /// </summary>
        /// <param name="instance">The instance being looked at.</param>
        /// <returns>true if the instance currently has a loaded or assigned value; otherwise, false.</returns>
        public virtual bool HasValue(object instance)
        {
            return true;
        }

        /// <summary>
        /// Sets the value as an object.
        /// </summary>
        /// <param name="instance">The instance into which to set the value.</param>
        /// <param name="value">The value to set.</param>
        public abstract void SetBoxedValue(ref object instance, object value);

        /// <summary>
        /// Gets the type of the member accessed by this accessor.
        /// </summary>
        /// <returns>
        /// The type of the member.
        /// </returns>
        public abstract Type Type { get; }
    }

    /// <summary>
    /// A strongly typed version of the ALinq.Mapping.MetaAccessor class.
    /// </summary>
    /// <typeparam name="TEntity">The type of the source.</typeparam>
    /// <typeparam name="TMember">The type of the member of that source.</typeparam>
    public abstract class MetaAccessor<TEntity, TMember> : MetaAccessor
    {
       /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.MetaAccessor&lt;TEntity,TMember&gt; class.
       /// </summary>
        protected MetaAccessor()
        {
        }

        /// <summary>
        /// Specifies an object on which to set a value or from which to get a value.
        /// </summary>
        /// <param name="instance">The instance from which to get the value or on which to set the value.</param>
        /// <returns>The boxed value of this instance.</returns>
        public override object GetBoxedValue(object instance)
        {
            return this.GetValue((TEntity)instance);
        }

        /// <summary>
        /// Specifies the strongly typed value.
        /// </summary>
        /// <param name="instance">The instance from which to get the value.</param>
        /// <returns>The value of this instance.</returns>
        public abstract TMember GetValue(TEntity instance);

        /// <summary>
        /// Specifies an instance on which to set the boxed value.
        /// </summary>
        /// <param name="instance">The instance into which to set the boxed value.</param>
        /// <param name="value">The value to set.</param>
        public override void SetBoxedValue(ref object instance, object value)
        {
            var local = (TEntity)instance;
            this.SetValue(ref local, (TMember)value);
            instance = local;
        }

        /// <summary>
        /// Specifies an instance on which to set the strongly typed value.
        /// </summary>
        /// <param name="instance">The instance into which to set the value.</param>
        /// <param name="value">The strongly typed value to set.</param>
        public abstract void SetValue(ref TEntity instance, TMember value);

        /// <summary>
        /// Gets the type of the member accessed by this accessor.
        /// </summary>
        /// <returns>
        /// The member type.
        /// </returns>
        public override Type Type
        {
            get
            {
                return typeof(TMember);
            }
        }
    }
}