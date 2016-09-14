using System;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.Mapping
{

    #region MyRegion
    //internal sealed class DynamicMetaDataMember : MetaDataMember
    //{
    //    // Fields
    //    private MetaAccessor accPublic;
    //    private MetaType declaringType;
    //    private object lockTarget = new object();
    //    private IndexerMemberInfo member;
    //    private int ordinal;
    //    private Type type;
    //    private string key;
    //    //private string columName;
    //    private ColumnAttribute colattr;

    //    // Methods
    //    //internal DynamicMetaDataMember(MetaType declaringType, IndexerMemberInfo mi)
    //    //    : this(declaringType, mi, declaringType.DataMembers.Count, TODO)
    //    //{

    //    //}
    //    internal DynamicMetaDataMember(MetaType declaringType, IndexerMemberInfo mi, int ordinal, ColumnAttribute colattr)
    //    {
    //        if (colattr == null)
    //            this.colattr = new ColumnAttribute();
    //        else
    //            this.colattr = colattr;

    //        this.declaringType = declaringType;
    //        this.member = mi;
    //        //this.ordinal = ordinal;
    //        this.type = TypeSystem.GetMemberType(mi);
    //        //this.columName = mi.Name;
    //        this.ordinal = ordinal;
    //    }

    //    private void InitAccessors()
    //    {
    //        if (this.accPublic == null)
    //        {
    //            lock (this.lockTarget)
    //            {
    //                if (this.accPublic == null)
    //                {
    //                    this.accPublic = MakeMemberAccessor(this.member.ReflectedType, this.member);
    //                }
    //            }
    //        }
    //    }

    //    public override bool IsDeclaredBy(MetaType metaType)
    //    {
    //        if (metaType == null)
    //        {
    //            throw SqlClient.Error.ArgumentNull("metaType");
    //        }
    //        return (metaType.Type == this.member.DeclaringType);
    //    }

    //    private static MetaAccessor MakeMemberAccessor(Type accessorType, MemberInfo mi)
    //    {
    //        FieldInfo fi = mi as FieldInfo;
    //        if (fi != null)
    //        {
    //            return FieldAccessor.Create(accessorType, fi);
    //        }
    //        PropertyInfo pi = (PropertyInfo)mi;
    //        return PropertyAccessor.Create(accessorType, pi, null);
    //    }

    //    // Properties
    //    public override MetaAssociation Association
    //    {
    //        get
    //        {
    //            return null;
    //        }
    //    }

    //    public override AutoSync AutoSync
    //    {
    //        get
    //        {
    //            return AutoSync.Never;
    //        }
    //    }

    //    public override bool CanBeNull
    //    {
    //        get
    //        {
    //            return colattr.CanBeNull;
    //        }
    //    }

    //    public override string DbType
    //    {
    //        get
    //        {
    //            return colattr.DbType;
    //        }
    //    }

    //    public override MetaType DeclaringType
    //    {
    //        get
    //        {
    //            return this.declaringType;
    //        }
    //    }

    //    public override MetaAccessor DeferredSourceAccessor
    //    {
    //        get
    //        {
    //            return null;
    //        }
    //    }

    //    public override MetaAccessor DeferredValueAccessor
    //    {
    //        get
    //        {
    //            return null;
    //        }
    //    }

    //    public override string Expression
    //    {
    //        get
    //        {
    //            return colattr.Expression;
    //        }
    //    }

    //    public override bool IsAssociation
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    public override bool IsDbGenerated
    //    {
    //        get
    //        {
    //            return colattr.IsDbGenerated;
    //        }
    //    }

    //    public override bool IsDeferred
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    public override bool IsDiscriminator
    //    {
    //        get
    //        {
    //            return colattr.IsDiscriminator;
    //        }
    //    }

    //    public override bool IsPersistent
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    public override bool IsPrimaryKey
    //    {
    //        get
    //        {
    //            return colattr.IsPrimaryKey;
    //        }
    //    }

    //    public override bool IsVersion
    //    {
    //        get
    //        {
    //            return colattr.IsVersion;
    //        }
    //    }

    //    public override MethodInfo LoadMethod
    //    {
    //        get
    //        {
    //            return null;
    //        }
    //    }

    //    public override string MappedName
    //    {
    //        get
    //        {
    //            if (string.IsNullOrEmpty(colattr.Name))
    //                return this.member.Name;

    //            return colattr.Name;
    //        }
    //    }

    //    public override MemberInfo Member
    //    {
    //        get
    //        {
    //            return this.member;
    //        }
    //    }

    //    public override MetaAccessor MemberAccessor
    //    {
    //        get
    //        {
    //            this.InitAccessors();
    //            return this.accPublic;
    //        }
    //    }

    //    public override string Name
    //    {
    //        get
    //        {
    //            return this.member.Name;
    //        }
    //    }

    //    public override int Ordinal
    //    {
    //        get
    //        {
    //            return this.ordinal;
    //        }
    //    }

    //    public override MetaAccessor StorageAccessor
    //    {
    //        get
    //        {
    //            this.InitAccessors();
    //            return this.accPublic;
    //        }
    //    }

    //    public override MemberInfo StorageMember
    //    {
    //        get
    //        {
    //            return this.member;
    //        }
    //    }

    //    public override Type Type
    //    {
    //        get
    //        {
    //            return this.type;
    //        }
    //    }

    //    public override UpdateCheck UpdateCheck
    //    {
    //        get
    //        {
    //            return colattr.UpdateCheck;
    //        }
    //    }

    //    internal void SetType(System.Type value)
    //    {
    //        this.type = value;
    //        this.member.SetPropertyType(value);
    //    }

    //    public override string ToString()
    //    {
    //        return (this.GetType().Name + ":" + this.Member);
    //    }
    //} 
    #endregion

}