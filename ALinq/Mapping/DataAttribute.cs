using System;

namespace ALinq.Mapping
{
    /// <summary>
    /// Provides members to describe attributes of data in columns.
    /// </summary>
    public abstract class DataAttribute : Attribute
    {
        // Fields
        private string name;
        private string storage;

        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.DataAttribute class.
        /// </summary>
        protected DataAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the name of a column.
        /// </summary>
        /// <returns>
        /// The name.
        /// </returns>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        /// <summary>
        /// Gets or sets a private storage field to hold the value from a column.
        /// </summary>
        /// <returns>
        /// The name of the storage field.
        /// </returns>
        public string Storage
        {
            get
            {
                return this.storage;
            }
            set
            {
                this.storage = value;
            }
        }
    }
}