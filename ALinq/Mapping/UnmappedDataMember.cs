using System;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal sealed class UnmappedDataMember : MetaDataMember
    {
        // Fields
        private MetaAccessor accPublic;
        private MetaType declaringType;
        private object lockTarget = new object();
        private MemberInfo member;
        private int ordinal;
        private Type type;

        // Methods
        internal UnmappedDataMember(MetaType declaringType, MemberInfo mi, int ordinal)
        {
            this.declaringType = declaringType;
            this.member = mi;
            this.ordinal = ordinal;
            this.type = TypeSystem.GetMemberType(mi);
        }

        private void InitAccessors()
        {
            if (this.accPublic == null)
            {
                lock (this.lockTarget)
                {
                    if (this.accPublic == null)
                    {
                        this.accPublic = MakeMemberAccessor(this.member.ReflectedType, this.member);
                    }
                }
            }
        }

        public override bool IsDeclaredBy(MetaType metaType)
        {
            if (metaType == null)
            {
                throw Error.ArgumentNull("metaType");
            }
            return (metaType.Type == this.member.DeclaringType);
        }

        private static MetaAccessor MakeMemberAccessor(Type accessorType, MemberInfo mi)
        {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null)
            {
                return FieldAccessor.Create(accessorType, fi);
            }
            PropertyInfo pi = (PropertyInfo)mi;
            return PropertyAccessor.Create(accessorType, pi, null);
        }

        // Properties
        public override MetaAssociation Association
        {
            get
            {
                return null;
            }
        }

        public override AutoSync AutoSync
        {
            get
            {
                return AutoSync.Never;
            }
        }

        public override bool CanBeNull
        {
            get
            {
                if (this.type.IsValueType)
                {
                    return TypeSystem.IsNullableType(this.type);
                }
                return true;
            }
        }

        public override string DbType
        {
            get
            {
                return null;
            }
        }

        public override MetaType DeclaringType
        {
            get
            {
                return this.declaringType;
            }
        }

        public override MetaAccessor DeferredSourceAccessor
        {
            get
            {
                return null;
            }
        }

        public override MetaAccessor DeferredValueAccessor
        {
            get
            {
                return null;
            }
        }

        public override string Expression
        {
            get
            {
                return null;
            }
        }

        public override bool IsAssociation
        {
            get
            {
                return false;
            }
        }

        public override bool IsDbGenerated
        {
            get
            {
                return false;
            }
        }

        public override bool IsDeferred
        {
            get
            {
                return false;
            }
        }

        public override bool IsDiscriminator
        {
            get
            {
                return false;
            }
        }

        public override bool IsPersistent
        {
            get
            {
                return false;
            }
        }

        public override bool IsPrimaryKey
        {
            get
            {
                return false;
            }
        }

        public override bool IsVersion
        {
            get
            {
                return false;
            }
        }

        public override MethodInfo LoadMethod
        {
            get
            {
                return null;
            }
        }

        public override string MappedName
        {
            get
            {
                return this.member.Name;
            }
        }

        public override MemberInfo Member
        {
            get
            {
                return this.member;
            }
        }

        public override MetaAccessor MemberAccessor
        {
            get
            {
                this.InitAccessors();
                return this.accPublic;
            }
        }

        public override string Name
        {
            get
            {
                return this.member.Name;
            }
        }

        public override int Ordinal
        {
            get
            {
                return this.ordinal;
            }
        }

        public override MetaAccessor StorageAccessor
        {
            get
            {
                this.InitAccessors();
                return this.accPublic;
            }
        }

        public override MemberInfo StorageMember
        {
            get
            {
                return this.member;
            }
        }

        public override Type Type
        {
            get
            {
                return this.type;
            }
        }

        public override UpdateCheck UpdateCheck
        {
            get
            {
                return UpdateCheck.Never;
            }
        }
    }
}