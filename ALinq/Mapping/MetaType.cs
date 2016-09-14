using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ALinq.Mapping
{
    /// <summary>
    /// Represents the mapping of a domain object type to the columns of a database table.
    /// </summary>
    public abstract class MetaType
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.MetaType class.
        /// </summary>
        protected MetaType()
        {
        }

        /// <summary>
        /// Gets the ALinq.Mapping.MetaDataMember associated with the specified member.
        /// </summary>
        /// <param name="member">The member for which the associated ALinq.Mapping.MetaDataMember is sought.</param>
        /// <returns>The ALinq.Mapping.MetaDataMember if one is associated with the specified member; otherwise, null.</returns>
        public abstract MetaDataMember GetDataMember(MemberInfo member);

        /// <summary>
        /// Gets the ALinq.Mapping.MetaType for an inheritance subtype.
        /// </summary>
        /// <param name="type">The subtype.</param>
        /// <returns>The ALinq.Mapping.MetaType for an inheritance subtype.</returns>
        public abstract MetaType GetInheritanceType(Type type);

        /// <summary>
        /// Gets the meta-type associated with the specified inheritance code.
        /// </summary>
        /// <param name="code">The inheritance code.</param>
        /// <returns>The meta-type associated with the specified inheritance code.</returns>
        public abstract MetaType GetTypeForInheritanceCode(object code);

        /// <summary>
        /// Gets an enumeration of all the associations.
        /// </summary>
        /// <returns>
        /// A collection of associations.
        /// </returns>
        public abstract ReadOnlyCollection<MetaAssociation> Associations { get; }

        /// <summary>
        /// Gets whether the underlying type can be instantiated as the result of a query.
        /// </summary>
        /// <returns>
        /// true if the underlying type can be instantiated as the result of a query; otherwise, false.
        /// </returns>
        public abstract bool CanInstantiate { get; }

        /// <summary>
        /// Gets an enumeration of all the data members (fields and properties).
        /// </summary>
        /// <returns>
        /// A collection of the data members.
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> DataMembers { get; }

        /// <summary>
        /// Gets the member that represents the auto-generated identity column.
        /// </summary>
        /// <returns>
        /// The member that represents the auto-generated identity column, or null if there is no auto-generated identity column.
        /// </returns>
        public abstract MetaDataMember DBGeneratedIdentityMember { get; }

        /// <summary>
        /// Gets an enumeration of the immediate derived types in an inheritance hierarchy.
        /// </summary>
        /// <returns>
        /// An enumeration of meta-types.
        /// </returns>
        public abstract ReadOnlyCollection<MetaType> DerivedTypes { get; }

        /// <summary>
        /// Gets the member that represents the inheritance discriminator column.
        /// </summary>
        /// <returns>
        /// The member that represents the inheritance discriminator column, or null if there is none.
        /// </returns>
        public abstract MetaDataMember Discriminator { get; }

        /// <summary>
        /// Gets a value that indicates whether the current ALinq.Mapping.MetaType or any of its bases types has an OnLoaded method.
        /// </summary>
        /// <returns>
        /// true if the meta-type or any base meta-type has an OnLoaded method; otherwise, false.
        /// </returns>
        public abstract bool HasAnyLoadMethod { get; }

        /// <summary>
        /// Gets a value that indicates whether the ALinq.Mapping.MetaType or any of its bases types has an OnValidate method.
        /// </summary>
        /// <returns>
        /// true if the meta-type or any base meta-type has an OnValidate method; otherwise, false.
        /// </returns>
        public abstract bool HasAnyValidateMethod { get; }

        internal virtual bool HasAnySetDataContextMethod
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the type is part of a mapped inheritance hierarchy.
        /// </summary>
        /// <returns>
        /// true if the type is part of a mapped inheritance hierarchy; otherwise false.
        /// </returns>
        public abstract bool HasInheritance { get; }

        /// <summary>
        /// Gets a value indicating whether this type defines an inheritance code.
        /// </summary>
        /// <returns>
        /// true if this type defines an inheritance code; otherwise false.
        /// </returns>
        public abstract bool HasInheritanceCode { get; }

        /// <summary>
        /// Gets a value indicating whether the type has any persistent member that may require a test for optimistic concurrency conflicts.
        /// </summary>
        /// <returns>
        /// true if the type has any persistent member with an ALinq.Mapping.UpdateCheck policy other than ALinq.Mapping.UpdateCheck.Never; otherwise false.
        /// </returns>
        public abstract bool HasUpdateCheck { get; }

        /// <summary>
        /// Gets an enumeration of all the data members that define the unique identity of the type.
        /// </summary>
        /// <returns>
        /// An enumeration of members that define the unique identity of the type.
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> IdentityMembers { get; }

        /// <summary>
        /// Gets the base meta-type in the inheritance hierarchy.
        /// </summary>
        /// <returns>
        /// The base meta-type for the current inheritance hierarchy.
        /// </returns>
        public abstract MetaType InheritanceBase { get; }

        /// <summary>
        /// Gets a value indicating whether this type defines an inheritance code.
        /// </summary>
        /// <returns>
        /// true if this type defines an inheritance code; otherwise false.
        /// </returns>
        public abstract object InheritanceCode { get; }

        /// <summary>
        /// Gets a value indicating whether this type is used as the default of an inheritance hierarchy.
        /// </summary>
        /// <returns>
        /// true if this type is used as the default of an inheritance hierarchy; otherwise false.
        /// </returns>
        public abstract MetaType InheritanceDefault { get; }

        /// <summary>
        /// Gets the root type of the inheritance hierarchy.
        /// </summary>
        /// <returns>
        /// The root type.
        /// </returns>
        public abstract MetaType InheritanceRoot { get; }

        /// <summary>
        /// Gets a collection of all types that are defined by an inheritance hierarchy.
        /// </summary>
        /// <returns>
        /// A collection of meta-types in the current inheritance hierarchy.
        /// </returns>
        public abstract ReadOnlyCollection<MetaType> InheritanceTypes { get; }

        /// <summary>
        /// Gets a value indicating whether the ALinq.Mapping.MetaType is an entity type.
        /// </summary>
        /// <returns>
        /// true if the ALinq.Mapping.MetaType is an entity type; otherwise false.
        /// </returns>
        public abstract bool IsEntity { get; }

        /// <summary>
        /// Gets a value indicating whether this type is used as the default of an inheritance hierarchy.
        /// </summary>
        /// <returns>
        /// true if this type is used as the default of an inheritance hierarchy; otherwise false.
        /// </returns>
        public abstract bool IsInheritanceDefault { get; }

        /// <summary>
        /// Gets the ALinq.Mapping.MetaModel that contains this ALinq.Mapping.MetaType.
        /// </summary>
        /// <returns>
        /// The containing meta-model.
        /// </returns>
        public abstract MetaModel Model { get; }

        /// <summary>
        /// Gets the name of the ALinq.Mapping.MetaType.
        /// </summary>
        /// <returns>
        /// The name of the current meta-type.
        /// </returns>
        public abstract string Name { get; }

        /// <summary>
        /// Gets information about the OnLoaded method contained by this meta-type.
        /// </summary>
        /// <returns>
        /// A description of the OnLoaded method for this meta-type.
        /// </returns>
        public abstract MethodInfo OnLoadedMethod { get; }

        /// <summary>
        /// Gets information about the OnValidate method contained by this meta-type.
        /// </summary>
        /// <returns>
        /// A description of the OnValidate method for this meta-type.
        /// </returns>
        public abstract MethodInfo OnValidateMethod { get; }

        internal virtual MethodInfo SetDataContextMehod
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a collection of all the persistent data members.
        /// </summary>
        /// <returns>
        /// A collection of all the meta-data members in the current type.
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> PersistentDataMembers { get; }

        /// <summary>
        /// Gets the ALinq.Mapping.MetaTable that uses this ALinq.Mapping.MetaType for row definition.
        /// </summary>
        /// <returns>
        /// A meta-table that uses the current meta-type for its row definition.
        /// </returns>
        public abstract MetaTable Table { get; }

        /// <summary>
        /// Gets the underlying common language runtime (CLR) type.
        /// </summary>
        /// <returns>
        /// The associated CLR type.
        /// </returns>
        public abstract Type Type { get; }

        /// <summary>
        /// Gets a row-version or timestamp column for this ALinq.Mapping.MetaType.
        /// </summary>
        /// <returns>
        /// The meta-data member that represents the row-version or timestamp column for this meta-type, or null if there is none.
        /// </returns>
        public abstract MetaDataMember VersionMember { get; }


    }



}
