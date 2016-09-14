using System;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal sealed class AttributedMetaDataMember : MetaDataMember
    {
        // Fields
        private MetaAccessor accDefSource;
        private MetaAccessor accDefValue;
        private MetaAccessor accPrivate;
        private MetaAccessor accPublic;
        private AttributedMetaAssociation assoc;
        private readonly DataAttribute attr;
        private readonly AssociationAttribute attrAssoc;
        private readonly ColumnAttribute attrColumn;
        private readonly Type declaringType;
        private bool hasAccessors;
        private bool hasLoadMethod;
        private readonly bool isDeferred;
        private readonly bool isNullableType;
        private MethodInfo loadMethod;
        private readonly object locktarget = new object();
        private readonly MemberInfo member;
        private readonly AttributedMetaType metaType;
        private readonly int ordinal;
        private readonly MemberInfo storageMember;
        private readonly Type type;
        private IAttributeProvider attributeProvider;

        // Methods
        internal AttributedMetaDataMember(AttributedMetaType metaType, MemberInfo mi, int ordinal)
        {
            this.declaringType = mi.DeclaringType;
            this.metaType = metaType;
            this.member = mi;
            this.ordinal = ordinal;
            this.type = TypeSystem.GetMemberType(mi);
            this.isNullableType = TypeSystem.IsNullableType(this.type);

            this.attributeProvider = this.MetaModel.AttributeProvider;

            /*
            this.attrColumn = (ColumnAttribute)Attribute.GetCustomAttribute(mi, typeof(ColumnAttribute));
            this.attrAssoc = (AssociationAttribute)Attribute.GetCustomAttribute(mi, typeof(AssociationAttribute));
            */
            this.attrColumn = attributeProvider.GetColumnAttribute(mi);
            this.attrAssoc = attributeProvider.GetAssociationAttribute(mi);

            this.attr = (this.attrColumn != null) ? ((DataAttribute)attrColumn) : this.attrAssoc;
            if (attr != null && attr.Storage != null)
            {
                MemberInfo[] member = mi.DeclaringType.GetMember(this.attr.Storage, BindingFlags.NonPublic | BindingFlags.Instance);
                if (member == null || member.Length != 1)
                {
                    throw Mapping.Error.BadStorageProperty(this.attr.Storage, mi.DeclaringType, mi.Name);
                }
                this.storageMember = member[0];
            }
            Type entityType = (this.storageMember != null) ? TypeSystem.GetMemberType(this.storageMember) : this.type;
            this.isDeferred = this.IsDeferredType(entityType);
            if ((this.attrColumn != null && this.attrColumn.IsDbGenerated) &&
                (this.attrColumn.IsPrimaryKey && this.attrColumn.AutoSync != AutoSync.Default) &&
                (this.attrColumn.AutoSync != AutoSync.OnInsert))
            {
                throw Mapping.Error.IncorrectAutoSyncSpecification(mi.Name);
            }
        }

        internal AttributedMetaModel MetaModel
        {
            get { return (AttributedMetaModel) this.metaType.Model; }
        }

        private static MetaAccessor CreateAccessor(Type accessorType, params object[] args)
        {
            return (MetaAccessor)Activator.CreateInstance(accessorType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, args, null);
        }

        private void InitAccessors()
        {
            if (!this.hasAccessors)
            {
                lock (this.locktarget)
                {
                    if (!this.hasAccessors)
                    {
                        if (this.storageMember != null)
                        {
                            this.accPrivate = MakeMemberAccessor(this.member.ReflectedType, this.storageMember, null);
                            if (this.isDeferred)
                            {
                                MakeDeferredAccessors(this.member.ReflectedType, this.accPrivate, out this.accPrivate, out this.accDefValue, out this.accDefSource);
                            }
                            this.accPublic = MakeMemberAccessor(this.member.ReflectedType, this.member, this.accPrivate);
                        }
                        else
                        {
                            this.accPublic = this.accPrivate = MakeMemberAccessor(this.member.ReflectedType, this.member, null);
                            if (this.isDeferred)
                            {
                                MakeDeferredAccessors(this.member.ReflectedType, this.accPrivate, out this.accPrivate, out this.accDefValue, out this.accDefSource);
                            }
                        }
                        this.hasAccessors = true;
                    }
                }
            }
        }

        public override bool IsDeclaredBy(MetaType declaringMetaType)
        {
            if (declaringMetaType == null)
            {
                throw ALinq.Error.ArgumentNull("declaringMetaType");
            }
            return (declaringMetaType.Type == this.declaringType);
        }

        private bool IsDeferredType(Type entityType)
        {
            if ((entityType == null) || (entityType == typeof(object)))
            {
                return false;
            }
            if (!entityType.IsGenericType)
            {
                return false;
            }
            Type genericTypeDefinition = entityType.GetGenericTypeDefinition();
            if (((genericTypeDefinition != typeof(Link<>)) && !typeof(EntitySet<>).IsAssignableFrom(genericTypeDefinition)) && !typeof(EntityRef<>).IsAssignableFrom(genericTypeDefinition))
            {
                return this.IsDeferredType(entityType.BaseType);
            }
            return true;
        }

        private static void MakeDeferredAccessors(Type objectDeclaringType, MetaAccessor accessor, out MetaAccessor accessorValue, out MetaAccessor accessorDeferredValue, out MetaAccessor accessorDeferredSource)
        {
            if (accessor.Type.IsGenericType)
            {
                Type genericTypeDefinition = accessor.Type.GetGenericTypeDefinition();
                Type type2 = accessor.Type.GetGenericArguments()[0];
                if (genericTypeDefinition == typeof(Link<>))
                {
                    accessorValue = CreateAccessor(typeof(LinkValueAccessor<,>).MakeGenericType(new Type[] { objectDeclaringType, type2 }), new object[] { accessor });
                    accessorDeferredValue = CreateAccessor(typeof(LinkDefValueAccessor<,>).MakeGenericType(new Type[] { objectDeclaringType, type2 }), new object[] { accessor });
                    accessorDeferredSource = CreateAccessor(typeof(LinkDefSourceAccessor<,>).MakeGenericType(new Type[] { objectDeclaringType, type2 }), new object[] { accessor });
                    return;
                }
                if (typeof(EntityRef<>).IsAssignableFrom(genericTypeDefinition))
                {
                    accessorValue = CreateAccessor(typeof(EntityRefValueAccessor<,>).MakeGenericType(new Type[] { objectDeclaringType, type2 }), new object[] { accessor });
                    accessorDeferredValue = CreateAccessor(typeof(EntityRefDefValueAccessor<,>).MakeGenericType(new Type[] { objectDeclaringType, type2 }), new object[] { accessor });
                    accessorDeferredSource = CreateAccessor(typeof(EntityRefDefSourceAccessor<,>).MakeGenericType(new Type[] { objectDeclaringType, type2 }), new object[] { accessor });
                    return;
                }
                if (typeof(EntitySet<>).IsAssignableFrom(genericTypeDefinition))
                {
                    accessorValue = CreateAccessor(typeof(EntitySetValueAccessor<,>).MakeGenericType(new Type[] { objectDeclaringType, type2 }), new object[] { accessor });
                    accessorDeferredValue = CreateAccessor(typeof(EntitySetDefValueAccessor<,>).MakeGenericType(new Type[] { objectDeclaringType, type2 }), new object[] { accessor });
                    accessorDeferredSource = CreateAccessor(typeof(EntitySetDefSourceAccessor<,>).MakeGenericType(new Type[] { objectDeclaringType, type2 }), new object[] { accessor });
                    return;
                }
            }
            throw Mapping.Error.UnhandledDeferredStorageType(accessor.Type);
        }

        private static MetaAccessor MakeMemberAccessor(Type accessorType, MemberInfo mi, MetaAccessor storage)
        {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null)
            {
                return FieldAccessor.Create(accessorType, fi);
            }
            PropertyInfo pi = (PropertyInfo)mi;
            return PropertyAccessor.Create(accessorType, pi, storage);
        }

        public override string ToString()
        {
            return (this.DeclaringType.ToString() + ":" + this.Member.ToString());
        }

        // Properties
        public override MetaAssociation Association
        {
            get
            {
                if (this.IsAssociation && (this.assoc == null))
                {
                    lock (this.locktarget)
                    {
                        if (this.assoc == null)
                        {
                            this.assoc = new AttributedMetaAssociation(this, this.attrAssoc);
                        }
                    }
                }
                return this.assoc;
            }
        }

        public override AutoSync AutoSync
        {
            get
            {
                if (this.attrColumn != null)
                {
                    if (this.IsDbGenerated && this.IsPrimaryKey)
                    {
                        return AutoSync.OnInsert;
                    }
                    if (this.attrColumn.AutoSync != AutoSync.Default)
                    {
                        return this.attrColumn.AutoSync;
                    }
                    if (this.IsDbGenerated)
                    {
                        return AutoSync.Always;
                    }
                }
                return AutoSync.Never;
            }
        }

        public override bool CanBeNull
        {
            get
            {
                if (this.attrColumn != null)
                {
                    if (this.attrColumn.CanBeNullSet)
                    {
                        return this.attrColumn.CanBeNull;
                    }
                    if (!this.isNullableType)
                    {
                        return !this.type.IsValueType;
                    }
                }
                return true;
            }
        }

        public override string DbType
        {
            get
            {
                if (this.attrColumn != null)
                {
                    return this.attrColumn.DbType;
                }
                return null;
            }
        }

        public override MetaType DeclaringType
        {
            get
            {
                return this.metaType;
            }
        }

        public override MetaAccessor DeferredSourceAccessor
        {
            get
            {
                this.InitAccessors();
                return this.accDefSource;
            }
        }

        public override MetaAccessor DeferredValueAccessor
        {
            get
            {
                this.InitAccessors();
                return this.accDefValue;
            }
        }

        public override string Expression
        {
            get
            {
                if (this.attrColumn != null)
                {
                    return this.attrColumn.Expression;
                }
                return null;
            }
        }

        public override bool IsAssociation
        {
            get
            {
                return (this.attrAssoc != null);
            }
        }

        public override bool IsDbGenerated
        {
            get
            {
                return (((this.attrColumn != null) && (this.attrColumn.IsDbGenerated || !string.IsNullOrEmpty(this.attrColumn.Expression))) || this.IsVersion);
            }
        }

        public override bool IsDeferred
        {
            get
            {
                return this.isDeferred;
            }
        }

        public override bool IsDiscriminator
        {
            get
            {
                return ((this.attrColumn != null) && this.attrColumn.IsDiscriminator);
            }
        }

        public override bool IsPersistent
        {
            get
            {
                if (this.attrColumn == null)
                {
                    return (this.attrAssoc != null);
                }
                return true;
            }
        }

        public override bool IsPrimaryKey
        {
            get
            {
                return ((this.attrColumn != null) && this.attrColumn.IsPrimaryKey);
            }
        }

        public override bool IsVersion
        {
            get
            {
                return ((this.attrColumn != null) && this.attrColumn.IsVersion);
            }
        }

        public override MethodInfo LoadMethod
        {
            get
            {
                if (!this.hasLoadMethod && (this.IsDeferred || this.IsAssociation))
                {
                    this.loadMethod = MethodFinder.FindMethod(((AttributedMetaModel)this.metaType.Model).ContextType, "Load" + this.member.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, new Type[] { this.DeclaringType.Type });
                    this.hasLoadMethod = true;
                }
                return this.loadMethod;
            }
        }

        public override string MappedName
        {
            get
            {
                if ((this.attrColumn != null) && (this.attrColumn.Name != null))
                {
                    return this.attrColumn.Name;
                }
                if ((this.attrAssoc != null) && (this.attrAssoc.Name != null))
                {
                    return this.attrAssoc.Name;
                }
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
                return this.accPrivate;
            }
        }

        public override MemberInfo StorageMember
        {
            get
            {
                return this.storageMember;
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
                if (this.attrColumn != null)
                {
                    return this.attrColumn.UpdateCheck;
                }
                return UpdateCheck.Never;
            }
        }
    }
}