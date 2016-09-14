using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ALinq
{
    /// <summary>
    /// Provides a container to hold changes.
    /// </summary>
    public sealed class ChangeSet
    {
        // Fields
        private ReadOnlyCollection<object> deletes;
        private ReadOnlyCollection<object> inserts;
        private ReadOnlyCollection<object> updates;

        // Methods
        internal ChangeSet(ReadOnlyCollection<object> inserts, ReadOnlyCollection<object> deletes, ReadOnlyCollection<object> updates)
        {
            this.inserts = inserts;
            this.deletes = deletes;
            this.updates = updates;
        }

        /// <summary>
        /// Returns a string that represents the current ALinq.ChangeSet.
        /// </summary>
        /// <returns>A string that represents the current ALinq.ChangeSet.</returns>
        public override string ToString()
        {
            return ("{" + string.Format(CultureInfo.InvariantCulture, "Inserts: {0}, Deletes: {1}, Updates: {2}", new object[] { this.Inserts.Count, this.Deletes.Count, this.Updates.Count }) + "}");
        }

        /// <summary>
        /// Gets a list of entities that have been deleted from the ALinq.ChangeSet.
        /// </summary>
        public IList<object> Deletes
        {
            get
            {
                return this.deletes;
            }
        }

        /// <summary>
        /// Gets a list of entities that have been inserted into the ALinq.ChangeSet.
        /// </summary>
        public IList<object> Inserts
        {
            get
            {
                return this.inserts;
            }
        }

        /// <summary>
        /// Gets a list of entities that have been updated in the ALinq.ChangeSet.
        /// </summary>
        public IList<object> Updates
        {
            get
            {
                return this.updates;
            }
        }
    }
}