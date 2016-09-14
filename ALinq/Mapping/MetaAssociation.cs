using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ALinq.Mapping
{
    /// <summary>
    /// Represents an association relationship between two entity types.
    /// </summary>
    public abstract class MetaAssociation
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.MetaAssociation class.
        /// </summary>
        protected MetaAssociation()
        {
        }

        /// <summary>
        /// Gets whether the object should be deleted when the association is set to null.
        /// </summary>
        /// <returns>
        /// If true, the object is deleted when the association is set to null.
        /// </returns>
        public abstract bool DeleteOnNull { get; }

        /// <summary>
        /// Gets the behavior when the child is deleted.
        /// </summary>
        /// <returns>
        /// The string representing the rule, or null if no action is specified on delete.
        /// </returns>
        public abstract string DeleteRule { get; }

        /// <summary>
        /// Gets whether the other type is the parent of this type.
        /// </summary>
        /// <returns>
        /// Returns true is the other type is the parent of this type.
        /// </returns>
        public abstract bool IsForeignKey { get; }

        /// <summary>
        /// Gets whether the association represents a one-to-many relationship.
        /// </summary>
        /// <returns>
        /// Returns true if the association represents a one-to-many relationship.
        /// </returns>
        public abstract bool IsMany { get; }

        /// <summary>
        /// Gets whether the association can be null.
        /// </summary>
        /// <returns>
        /// Returns true if the association can be null.
        /// </returns>
        public abstract bool IsNullable { get; }

        /// <summary>
        /// Gets whether the association is unique.
        /// </summary>
        /// <returns>
        /// Returns true if the association is unique.
        /// </returns>
        public abstract bool IsUnique { get; }

        /// <summary>
        /// Gets a list of members that represents the values on the other side of the association.
        /// </summary>
        /// <returns>
        /// Returns a collection representing values on the other side of the association.
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> OtherKey { get; }

        /// <summary>
        /// Gets whether the ALinq.Mapping.MetaAssociation.OtherKey forms the identity of the other type.
        /// </summary>
        /// <returns>
        /// true if the ALinq.Mapping.MetaAssociation.OtherKey forms the identity (primary key) of the other type.
        /// </returns>
        public abstract bool OtherKeyIsPrimaryKey { get; }

        /// <summary>
        /// Gets the member on the other side of this association that represents the reverse association.
        /// </summary>
        /// <returns>
        /// The member on the other side.
        /// </returns>
        public abstract MetaDataMember OtherMember { get; }

        /// <summary>
        /// Gets the type on the other side of the association.
        /// </summary>
        /// <returns>
        /// The type.
        /// </returns>
        public abstract MetaType OtherType { get; }

        /// <summary>
        /// Gets a list of members representing the values on this side of the association.
        /// </summary>
        /// <returns>
        /// A collection.
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> ThisKey { get; }

        /// <summary>
        /// Gets whether ALinq.Mapping.MetaAssociation.ThisKey forms the identity of this type.
        /// </summary>
        /// <returns>
        /// true if ALinq.Mapping.MetaAssociation.ThisKey forms the identity (primary key) of the association.
        /// </returns>
        public abstract bool ThisKeyIsPrimaryKey { get; }

        /// <summary>
        /// Gets the member on this side that represents the association.
        /// </summary>
        /// <returns>
        /// The member.
        /// </returns>
        public abstract MetaDataMember ThisMember { get; }
    }


}
