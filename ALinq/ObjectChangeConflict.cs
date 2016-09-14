using System.Collections.Generic;
using System.Collections.ObjectModel;
using ALinq;
using ALinq.Mapping;
using System.Linq;

namespace ALinq
{
    /// <summary>
    /// Represents an update attempt with one or more optimistic concurrency conflicts.
    /// </summary>
    public sealed class ObjectChangeConflict
    {
        // Fields
        private object database;
        private bool? isDeleted;
        private bool isResolved;
        private ReadOnlyCollection<MemberChangeConflict> memberConflicts;
        private readonly object original;
        private readonly ChangeConflictSession session;
        private readonly TrackedObject trackedObject;

        // Methods
        internal ObjectChangeConflict(ChangeConflictSession session, TrackedObject trackedObject)
        {
            this.session = session;
            this.trackedObject = trackedObject;
            original = trackedObject.CreateDataCopy(trackedObject.Original);
        }

        internal ObjectChangeConflict(ChangeConflictSession session, TrackedObject trackedObject, bool isDeleted)
            : this(session, trackedObject)
        {
            this.isDeleted = isDeleted;
        }

        private static bool AreEqual(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
            {
                return false;
            }
            int index = 0;
            int length = a1.Length;
            while (index < length)
            {
                if (a1[index] != a2[index])
                {
                    return false;
                }
                index++;
            }
            return true;
        }

        private static bool AreEqual(char[] a1, char[] a2)
        {
            if (a1.Length != a2.Length)
            {
                return false;
            }
            int index = 0;
            int length = a1.Length;
            while (index < length)
            {
                if (a1[index] != a2[index])
                {
                    return false;
                }
                index++;
            }
            return true;
        }

        private static bool AreEqual(MetaDataMember member, object v1, object v2)
        {
            if ((v1 == null) && (v2 == null))
            {
                return true;
            }
            if ((v1 == null) || (v2 == null))
            {
                return false;
            }
            if (member.Type == typeof (char[]))
            {
                return AreEqual((char[]) v1, (char[]) v2);
            }
            if (member.Type == typeof (byte[]))
            {
                return AreEqual((byte[]) v1, (byte[]) v2);
            }
            return Equals(v1, v2);
        }

        private bool HasMemberConflict(MetaDataMember member)
        {
            object boxedValue = member.StorageAccessor.GetBoxedValue(original);
            if (!member.DeclaringType.Type.IsAssignableFrom(database.GetType()))
            {
                return false;
            }
            object obj3 = member.StorageAccessor.GetBoxedValue(database);
            return !AreEqual(member, boxedValue, obj3);
        }

        internal void OnMemberResolved()
        {
            if (!IsResolved && (memberConflicts.AsEnumerable().Count(m => m.IsResolved) == memberConflicts.Count))
            {
                Resolve(RefreshMode.KeepCurrentValues, false);
            }
        }

        /// <summary>
        /// Resolves member conflicts by keeping current values and resetting the baseline original values to match the more recent database values.
        /// </summary>
        public void Resolve()
        {
            Resolve(RefreshMode.KeepCurrentValues, true);
        }

        /// <summary>
        /// Resolves member conflicts by using the specified ALinq.RefreshMode.
        /// </summary>
        /// <param name="refreshMode">The appropriate option from ALinq.RefreshMode.</param>
        public void Resolve(RefreshMode refreshMode)
        {
            Resolve(refreshMode, false);
        }

        /// <summary>
        /// Resolve member conflicts keeping current values and resetting the baseline original values.
        /// </summary>
        /// <param name="refreshMode">The appropriate option from ALinq.RefreshMode.</param>
        /// <param name="autoResolveDeletes">When true, automatically resolves conflicts resulting from a modified object that is no longer in the database.</param>
        public void Resolve(RefreshMode refreshMode, bool autoResolveDeletes)
        {
            if (autoResolveDeletes && IsDeleted)
            {
                ResolveDelete();
            }
            else
            {
                if (Database == null)
                {
                    throw Error.RefreshOfDeletedObject();
                }
                trackedObject.Refresh(refreshMode, Database);
                isResolved = true;
            }
        }

        private void ResolveDelete()
        {
            if (!trackedObject.IsDeleted)
            {
                trackedObject.ConvertToDeleted();
            }
            trackedObject.AcceptChanges();
            isResolved = true;
        }

        // Properties
        internal object Database
        {
            get
            {
                if (database == null)
                {
                    DataContext refreshContext = session.RefreshContext;
                    object[] keyValues = CommonDataServices.GetKeyValues(trackedObject.Type, original);
                    database = refreshContext.Services.GetObjectByKey(trackedObject.Type, keyValues);
                }
                return database;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the object in conflict has been deleted from the database.
        /// </summary>
        /// <returns>
        /// True if the object has been deleted.
        /// </returns>
        public bool IsDeleted
        {
            get
            {
                if (isDeleted.HasValue)
                {
                    return isDeleted.Value;
                }
                return (Database == null);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the conflicts for this object have already been resolved.
        /// </summary>
        /// <returns>
        /// True if the conflicts have been resolved.
        /// </returns>
        public bool IsResolved
        {
            get { return isResolved; }
        }

        /// <summary>
        /// Gets a collection of all member conflicts that caused the update to fail.
        /// </summary>
        /// <returns>
        /// A collection of member conflicts.
        /// </returns>
        public ReadOnlyCollection<MemberChangeConflict> MemberConflicts
        {
            get
            {
                if (memberConflicts == null)
                {
                    var list = new List<MemberChangeConflict>();
                    if (Database != null)
                    {
                        foreach (MetaDataMember member in trackedObject.Type.PersistentDataMembers)
                        {
                            if (!member.IsAssociation && HasMemberConflict(member))
                            {
                                list.Add(new MemberChangeConflict(this, member));
                            }
                        }
                    }
                    memberConflicts = list.AsReadOnly();
                }
                return memberConflicts;
            }
        }

        /// <summary>
        /// Gets the object in conflict.
        /// </summary>
        /// <returns>
        /// The object in conflict.
        /// </returns>
        public object Object
        {
            get { return trackedObject.Current; }
        }

        internal object Original
        {
            get { return original; }
        }

        internal ChangeConflictSession Session
        {
            get { return session; }
        }

        internal TrackedObject TrackedObject
        {
            get { return trackedObject; }
        }
    }
}