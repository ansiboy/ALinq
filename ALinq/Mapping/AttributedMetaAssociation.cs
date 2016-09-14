using System;
using System.Collections.ObjectModel;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal class AttributedMetaAssociation : MetaAssociationImpl
    {
        // Fields
        private readonly bool deleteOnNull;
        private readonly string deleteRule;
        private readonly bool isForeignKey;
        private readonly bool isMany;
        private readonly bool isNullable = true;
        private readonly bool isUnique;
        private readonly ReadOnlyCollection<MetaDataMember> otherKey;
        private readonly bool otherKeyIsPrimaryKey;
        private readonly MetaDataMember otherMember;
        private readonly MetaType otherType;
        private readonly ReadOnlyCollection<MetaDataMember> thisKey;
        private readonly bool thisKeyIsPrimaryKey;
        private readonly AttributedMetaDataMember thisMember;
        private IAttributeProvider attributeProvider;

        // Methods
        internal AttributedMetaAssociation(AttributedMetaDataMember member, AssociationAttribute attr)
        {
            this.attributeProvider = member.MetaModel.AttributeProvider;

            this.thisMember = member;
            this.isMany = TypeSystem.IsSequenceType(thisMember.Type);
            Type type = this.isMany ? TypeSystem.GetElementType(thisMember.Type) : thisMember.Type;
            this.otherType = this.thisMember.DeclaringType.Model.GetMetaType(type);
            this.thisKey = (attr.ThisKey != null) ? MakeKeys(thisMember.DeclaringType, attr.ThisKey) : thisMember.DeclaringType.IdentityMembers;
            this.otherKey = (attr.OtherKey != null) ? MakeKeys(otherType, attr.OtherKey) : this.otherType.IdentityMembers;
            this.thisKeyIsPrimaryKey = AreEqual(this.thisKey, thisMember.DeclaringType.IdentityMembers);
            this.otherKeyIsPrimaryKey = AreEqual(this.otherKey, otherType.IdentityMembers);
            this.isForeignKey = attr.IsForeignKey;
            this.isUnique = attr.IsUnique;
            this.deleteRule = attr.DeleteRule;
            this.deleteOnNull = attr.DeleteOnNull;
            foreach (MetaDataMember member2 in this.thisKey)
            {
                if (!member2.CanBeNull)
                {
                    this.isNullable = false;
                    break;
                }
            }
            if (this.deleteOnNull && ((!this.isForeignKey || this.isMany) || this.isNullable))
            {
                throw Mapping.Error.InvalidDeleteOnNullSpecification(member);
            }
            if (((this.thisKey.Count != this.otherKey.Count) && (this.thisKey.Count > 0)) && (this.otherKey.Count > 0))
            {
                throw Mapping.Error.MismatchedThisKeyOtherKey(member.Name, member.DeclaringType.Name);
            }
            foreach (MetaDataMember member3 in this.otherType.PersistentDataMembers)
            {
                /* var customAttribute = (AssociationAttribute)Attribute.GetCustomAttribute(member3.Member, typeof(AssociationAttribute)); */
                var customAttribute = this.attributeProvider.GetAssociationAttribute(member3.Member);

                if (((customAttribute != null) && (member3 != this.thisMember)) && (customAttribute.Name == attr.Name))
                {
                    this.otherMember = member3;
                    break;
                }
            }
        }

        // Properties
        public override bool DeleteOnNull
        {
            get
            {
                return this.deleteOnNull;
            }
        }

        public override string DeleteRule
        {
            get
            {
                return this.deleteRule;
            }
        }

        public override bool IsForeignKey
        {
            get
            {
                return this.isForeignKey;
            }
        }

        public override bool IsMany
        {
            get
            {
                return this.isMany;
            }
        }

        public override bool IsNullable
        {
            get
            {
                return this.isNullable;
            }
        }

        public override bool IsUnique
        {
            get
            {
                return this.isUnique;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> OtherKey
        {
            get
            {
                return this.otherKey;
            }
        }

        public override bool OtherKeyIsPrimaryKey
        {
            get
            {
                return this.otherKeyIsPrimaryKey;
            }
        }

        public override MetaDataMember OtherMember
        {
            get
            {
                return this.otherMember;
            }
        }

        public override MetaType OtherType
        {
            get
            {
                return this.otherType;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> ThisKey
        {
            get
            {
                return this.thisKey;
            }
        }

        public override bool ThisKeyIsPrimaryKey
        {
            get
            {
                return this.thisKeyIsPrimaryKey;
            }
        }

        public override MetaDataMember ThisMember
        {
            get
            {
                return this.thisMember;
            }
        }
    }
}