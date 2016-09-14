using System;
using System.Collections;
using System.Collections.Generic;
using ALinq;

namespace ALinq
{
    /// <summary>
    /// Returns a collection of objects involved in concurrency conflicts.
    /// </summary>
    public sealed class ChangeConflictCollection : ICollection<ObjectChangeConflict>, ICollection
    {
        // Fields
        private List<ObjectChangeConflict> conflicts = new List<ObjectChangeConflict>();

        // Methods
        internal ChangeConflictCollection()
        {
        }

        /// <summary>
        /// Removes all conflicts from the collection.
        /// </summary>
        public void Clear()
        {
            this.conflicts.Clear();
        }

        /// <summary>
        /// Specifies whether a given conflict is a member of the collection.
        /// </summary>
        /// <param name="item">The specified conflict.</param>
        /// <returns>Returns true if the specified conflict is a member of the collection.</returns>
        public bool Contains(ObjectChangeConflict item)
        {
            return this.conflicts.Contains(item);
        }

        /// <summary>
        /// For a description of this member, see System.Collections.ICollection.CopyTo(System.Array,System.Int32).
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The array index where the copy is to start.</param>
        public void CopyTo(ObjectChangeConflict[] array, int arrayIndex)
        {
            this.conflicts.CopyTo(array, arrayIndex);
        }

        internal void Fill(List<ObjectChangeConflict> conflictList)
        {
            this.conflicts = conflictList;
        }

        /// <summary>
        /// Returns the enumerator for the collection.
        /// </summary>
        /// <returns>An enumerator for the collection.</returns>
        public IEnumerator<ObjectChangeConflict> GetEnumerator()
        {
            return this.conflicts.GetEnumerator();
        }

        /// <summary>
        /// Specifies whether the specified conflict is removed from the collection.
        /// </summary>
        /// <param name="item">The conflict to remove.</param>
        /// <returns>Returns true if the ALinq.ObjectChangeConflict is removed from the collection.</returns>
        public bool Remove(ObjectChangeConflict item)
        {
            return this.conflicts.Remove(item);
        }

        /// <summary>
        /// Resolves all conflicts in the collection by using the specified strategy.
        /// </summary>
        /// <param name="mode">One of the options available in ALinq.RefreshMode.</param>
        public void ResolveAll(RefreshMode mode)
        {
            this.ResolveAll(mode, true);
        }

        /// <summary>
        /// Resolves all conflicts in the collection by using the specified strategy.
        /// </summary>
        /// <param name="mode">The strategy to use to resolve the conflict.</param>
        /// <param name="autoResolveDeletes">If true, automatically resolves conflicts that result from a modified object that is no longer in the database.</param>
        public void ResolveAll(RefreshMode mode, bool autoResolveDeletes)
        {
            foreach (ObjectChangeConflict conflict in this.conflicts)
            {
                if (!conflict.IsResolved)
                {
                    conflict.Resolve(mode, autoResolveDeletes);
                }
            }
        }

        void ICollection<ObjectChangeConflict>.Add(ObjectChangeConflict item)
        {
            throw Error.CannotAddChangeConflicts();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            conflicts.CopyTo((ObjectChangeConflict[])array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return conflicts.GetEnumerator();
        }

        /// <summary>
        /// Returns the number of conflicts in the collection
        /// </summary>
        public int Count
        {
            get
            {
                return this.conflicts.Count;
            }
        }

        /// <summary>
        /// Returns an item in conflict.
        /// </summary>
        /// <param name="index">Index in the collection of the item in conflict.</param>
        /// <returns>An ALinq.ObjectChangeConflict representing the item in conflict.</returns>
        public ObjectChangeConflict this[int index]
        {
            get
            {
                return this.conflicts[index];
            }
        }

        bool ICollection<ObjectChangeConflict>.IsReadOnly
        {
            get
            {
                return true;
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
                return null;
            }
        }
    }


}
