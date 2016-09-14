using System;

namespace ALinq.Mapping
{
    /// <summary>
    /// Associates a class with a column in a database table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ColumnAttribute : DataAttribute
    {
        // Fields
        private bool canBeNull = true;
        private UpdateCheck check = UpdateCheck.Always;

        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.ColumnAttribute class.
        /// </summary>
        public ColumnAttribute()
        {
            
        }

        /// <summary>
        /// Gets or sets the ALinq.Mapping.AutoSync enumeration.
        /// </summary>
        /// <returns>
        /// The ALinq.Mapping.AutoSync value.
        /// </returns>
        public AutoSync AutoSync { get; set; }

        /// <summary>
        /// Gets or sets whether a column can contain null values.
        /// </summary>
        /// <returns>
        /// Default = true.
        /// </returns>
        public bool CanBeNull
        {
            get
            {
                return canBeNull;
            }
            set
            {
                CanBeNullSet = true;
                canBeNull = value;
            }
        }

        internal bool CanBeNullSet { get; private set; }

        /// <summary>
        /// Gets or sets the type of the database column.
        /// </summary>
        public string DbType { get; set; }

        /// <summary>
        /// Gets or sets whether a column is a computed column in a database.
        /// </summary>
        /// <returns>
        /// Default = empty.
        /// </returns>
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets whether a column contains values that the database auto-generates.
        /// </summary>
        /// <returns>
        /// Default = false.
        /// </returns>
        public bool IsDbGenerated { get; set; }

        /// <summary>
        /// Gets or sets whether a column contains a discriminator value for a LINQ to SQL inheritance hierarchy.
        /// </summary>
        /// <returns>
        /// Default = false.
        /// </returns>
        public bool IsDiscriminator { get; set; }

        /// <summary>
        /// Gets or sets whether this class member represents a column that is part or all of the primary key of the table.
        /// </summary>
        /// <returns>
        /// Default = false.
        /// </returns>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets whether the column type of the member is a database timestamp or version number.
        /// </summary>
        /// <returns>
        /// Default = false.
        /// </returns>
        public bool IsVersion { get; set; }

        /// <summary>
        /// Gets or sets how LINQ to SQL approaches the detection of optimistic concurrency conflicts.
        /// </summary>
        /// <returns>
        /// Default = ALinq.Mapping.UpdateCheck.Always, unless ALinq.Mapping.ColumnAttribute.IsVersion is true for a member.  Other values are ALinq.Mapping.UpdateCheck.Never and ALinq.Mapping.UpdateCheck.WhenChanged.
        /// </returns>
        public UpdateCheck UpdateCheck
        {
            get
            {
                return check;
            }
            set
            {
                check = value;
            }
        }
    }
}
