using ALinq;
using ALinq.Mapping;
using System.Reflection;

namespace ALinq
{
    /// <summary>
    /// Represents a situation in which an attempted update fails because member values have been updated since the client last read them.
    /// </summary>
    public sealed class MemberChangeConflict
    {
        // Fields
        private readonly ObjectChangeConflict conflict;
        private readonly object currentValue;
        private readonly object databaseValue;
        private bool isResolved;
        private readonly MetaDataMember metaMember;
        private readonly object originalValue;

        // Methods
        internal MemberChangeConflict(ObjectChangeConflict conflict, MetaDataMember metaMember)
        {
            this.conflict = conflict;
            this.metaMember = metaMember;
            originalValue = metaMember.StorageAccessor.GetBoxedValue(conflict.Original);
            databaseValue = metaMember.StorageAccessor.GetBoxedValue(conflict.Database);
            currentValue = metaMember.StorageAccessor.GetBoxedValue(conflict.TrackedObject.Current);
        }

        /// <summary>
        /// Uses a ALinq.RefreshMode parameter to automatically specify the value to set as the current value for the member in conflict.
        /// </summary>
        /// <param name="refreshMode">See ALinq.RefreshMode.</param>
        public void Resolve(RefreshMode refreshMode)
        {
            conflict.TrackedObject.RefreshMember(metaMember, refreshMode, databaseValue);
            isResolved = true;
            conflict.OnMemberResolved();
        }

        /// <summary>
        /// Specifies the value to set as the current value for the member in conflict.
        /// </summary>
        /// <param name="value">The value to set as the current value.</param>
        public void Resolve(object value)
        {
            conflict.TrackedObject.RefreshMember(metaMember, RefreshMode.OverwriteCurrentValues, value);
            isResolved = true;
            conflict.OnMemberResolved();
        }

        /// <summary>
        /// Gets the current value of the member in conflict.
        /// </summary>
        /// <returns>
        /// The object in conflict.
        /// </returns>
        public object CurrentValue
        {
            get { return currentValue; }
        }

        /// <summary>
        /// Gets the database value of the member in conflict.
        /// </summary>
        /// <returns>
        /// The value of the object in conflict.
        /// </returns>
        public object DatabaseValue
        {
            get { return databaseValue; }
        }

        /// <summary>
        /// Gets a value that indicates whether the member data has been changed since the last database read or refresh.
        /// </summary>
        /// <returns>
        /// True if the member data has been changed.
        /// </returns>
        public bool IsModified
        {
            get { return conflict.TrackedObject.HasChangedValue(metaMember); }
        }

        /// <summary>
        /// Gets a value that indicates whether the conflict has been resolved.
        /// </summary>
        /// <returns>
        /// True if the conflict has been resolved.
        /// </returns>
        public bool IsResolved
        {
            get { return isResolved; }
        }

        /// <summary>
        /// Gets metadata information about the member in conflict.
        /// </summary>
        /// <returns>
        /// Information about the member in conflict.
        /// </returns>
        public MemberInfo Member
        {
            get { return metaMember.Member; }
        }

        /// <summary>
        /// Gets the original value of the member in conflict.
        /// </summary>
        /// <returns>
        /// The original value of the member in conflict.
        /// </returns>
        public object OriginalValue
        {
            get { return originalValue; }
        }
    }
}