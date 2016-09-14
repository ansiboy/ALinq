using System;

namespace ALinq.Mapping
{
    /// <summary>
    /// Designates a property to represent a database association, such as a foreign key relationship.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AssociationAttribute : DataAttribute
    {
        // Fields
        private bool deleteOnNull;
        private string deleteRule;
        private bool isForeignKey;
        private bool isUnique;
        private string otherKey;
        private string thisKey;

        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.AssociationAttribute class.
        /// </summary>
        public AssociationAttribute()
        {
            
        }

        /// <summary>
        /// When placed on a 1:1 association whose foreign key members are all non-nullable, deletes the object when the association is set to null.
        /// </summary>
        /// <returns>
        /// Setting to True deletes the object. The default value is False.
        /// </returns>
        public bool DeleteOnNull
        {
            get
            {
                return this.deleteOnNull;
            }
            set
            {
                this.deleteOnNull = value;
            }
        }

        /// <summary>
        /// Gets or sets delete behavior for an association.
        /// </summary>
        /// <returns>
        /// A string representing the rule.
        /// </returns>
        public string DeleteRule
        {
            get
            {
                return this.deleteRule;
            }
            set
            {
                this.deleteRule = value;
            }
        }

        /// <summary>
        /// Gets or sets the member as the foreign key in an association representing a database relationship.
        /// </summary>
        /// <returns>
        /// Default = false.
        /// </returns>
        public bool IsForeignKey
        {
            get
            {
                return this.isForeignKey;
            }
            set
            {
                this.isForeignKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the indication of a uniqueness constraint on the foreign key.
        /// </summary>
        /// <returns>
        /// Default = false.
        /// </returns>
        public bool IsUnique
        {
            get
            {
                return this.isUnique;
            }
            set
            {
                this.isUnique = value;
            }
        }

        /// <summary>
        /// Gets or sets one or more members of the target entity class as key values on the other side of the association.
        /// </summary>
        /// <returns>
        /// Default = Id of the related class.
        /// </returns>
        public string OtherKey
        {
            get
            {
                return this.otherKey;
            }
            set
            {
                this.otherKey = value;
            }
        }

        /// <summary>
        /// Gets or sets members of this entity class to represent the key values on this side of the association.
        /// </summary>
        /// <returns>
        /// Default = Id of the containing class.
        /// </returns>
        public string ThisKey
        {
            get
            {
                return this.thisKey;
            }
            set
            {
                this.thisKey = value;
            }
        }
    }
}