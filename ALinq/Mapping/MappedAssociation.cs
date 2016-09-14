using System;
using System.Collections.ObjectModel;
using ALinq.SqlClient;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
internal class MappedAssociation : MetaAssociationImpl
{
    // Fields
    private AssociationMapping assocMap;
    private bool isForeignKey;
    private bool isMany;
    private bool isNullable;
    private ReadOnlyCollection<MetaDataMember> otherKey;
    private bool otherKeyIsPrimaryKey;
    private MetaDataMember otherMember;
    private MetaType otherType;
    private ReadOnlyCollection<MetaDataMember> thisKey;
    private bool thisKeyIsPrimaryKey;
    private MappedDataMember thisMember;

    // Methods
    internal MappedAssociation(MappedDataMember mm, AssociationMapping assocMap)
    {
        this.thisMember = mm;
        this.assocMap = assocMap;
        this.Init();
        this.InitOther();
        if (((this.thisKey.Count != this.otherKey.Count) && (this.thisKey.Count > 0)) && (this.otherKey.Count > 0))
        {
            throw Error.MismatchedThisKeyOtherKey(this.thisMember.Name, this.thisMember.DeclaringType.Name);
        }
    }

    private void Init()
    {
        this.isMany = TypeSystem.IsSequenceType(this.thisMember.Type);
        this.thisKey = (this.assocMap.ThisKey != null) ? MetaAssociationImpl.MakeKeys(this.thisMember.DeclaringType, this.assocMap.ThisKey) : this.thisMember.DeclaringType.IdentityMembers;
        this.thisKeyIsPrimaryKey = MetaAssociationImpl.AreEqual(this.thisKey, this.thisMember.DeclaringType.IdentityMembers);
        this.isForeignKey = this.assocMap.IsForeignKey;
        this.isNullable = true;
        foreach (MetaDataMember member in this.thisKey)
        {
            if (member == null)
            {
                throw Error.UnexpectedNull("MetaDataMember");
            }
            if (!member.CanBeNull)
            {
                this.isNullable = false;
                break;
            }
        }
        if (this.assocMap.DeleteOnNull && ((!this.isForeignKey || this.isMany) || this.isNullable))
        {
            throw Error.InvalidDeleteOnNullSpecification(this.thisMember);
        }
    }

    private void InitOther()
    {
        if (this.otherType == null)
        {
            Type type = this.isMany ? TypeSystem.GetElementType(this.thisMember.Type) : this.thisMember.Type;
            this.otherType = this.thisMember.DeclaringType.Model.GetMetaType(type);
            this.otherKey = (this.assocMap.OtherKey != null) ? MetaAssociationImpl.MakeKeys(this.otherType, this.assocMap.OtherKey) : this.otherType.IdentityMembers;
            this.otherKeyIsPrimaryKey = MetaAssociationImpl.AreEqual(this.otherKey, this.otherType.IdentityMembers);
            foreach (MetaDataMember member in this.otherType.DataMembers)
            {
                if ((member.IsAssociation && (member != this.thisMember)) && (member.MappedName == this.thisMember.MappedName))
                {
                    this.otherMember = member;
                    break;
                }
            }
        }
    }

    // Properties
    public override bool DeleteOnNull
    {
        get
        {
            return this.assocMap.DeleteOnNull;
        }
    }

    public override string DeleteRule
    {
        get
        {
            return this.assocMap.DeleteRule;
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
            return this.assocMap.IsUnique;
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