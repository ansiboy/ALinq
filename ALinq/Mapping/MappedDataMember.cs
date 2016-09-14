using System;
using System.Collections.Generic;
using System.Reflection;
using ALinq.Mapping;
using ALinq.SqlClient;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
    internal class EntitySetValueAccessor<T, V> : MetaAccessor<T, EntitySet<V>> where V : class
    {
        // Fields
        private MetaAccessor<T, EntitySet<V>> acc;

        // Methods
        internal EntitySetValueAccessor(MetaAccessor<T, EntitySet<V>> acc)
        {
            this.acc = acc;
        }

        public override EntitySet<V> GetValue(T instance)
        {
            return this.acc.GetValue(instance);
        }

        public override bool HasAssignedValue(object instance)
        {
            EntitySet<V> set = this.acc.GetValue((T)instance);
            return ((set != null) && set.HasAssignedValues);
        }

        public override bool HasLoadedValue(object instance)
        {
            EntitySet<V> set = this.acc.GetValue((T)instance);
            return ((set != null) && set.HasLoadedValues);
        }

        public override bool HasValue(object instance)
        {
            EntitySet<V> set = this.acc.GetValue((T)instance);
            return ((set != null) && set.HasValues);
        }

        public override void SetValue(ref T instance, EntitySet<V> value)
        {
            EntitySet<V> set = this.acc.GetValue(instance);
            if (set == null)
            {
                set = new EntitySet<V>();
                this.acc.SetValue(ref instance, set);
            }
            set.Assign(value);
        }
    }

 

    internal sealed class MappedDataMember : MetaDataMember
    {
        // Fields
        private MetaAccessor accDefSource;
        private MetaAccessor accDefValue;
        private MetaAccessor accPrivate;
        private MetaAccessor accPublic;
        private MappedAssociation assoc;
        private AutoSync autoSync = AutoSync.Never;
        private bool canBeNull = true;
        private string dbType;
        private MetaType declaringType;
        private string expression;
        private bool hasAccessors;
        private bool hasLoadMethod;
        private bool isDBGenerated;
        private bool isDeferred;
        private bool isDiscriminator;
        private bool isNullableType;
        private bool isPrimaryKey;
        private bool isVersion;
        private MethodInfo loadMethod;
        private object locktarget = new object();
        private string mappedName;
        private MemberInfo member;
        private MemberMapping memberMap;
        private int ordinal;
        private MemberInfo storageMember;
        private Type type;
        private UpdateCheck updateCheck = UpdateCheck.Never;

        // Methods
        internal MappedDataMember(MetaType declaringType, MemberInfo mi, MemberMapping map, int ordinal)
        {
            this.declaringType = declaringType;
            this.member = mi;
            this.ordinal = ordinal;
            this.type = TypeSystem.GetMemberType(mi);
            this.isNullableType = TypeSystem.IsNullableType(this.type);
            this.memberMap = map;
            if ((this.memberMap != null) && (this.memberMap.StorageMemberName != null))
            {
                MemberInfo[] member = mi.DeclaringType.GetMember(this.memberMap.StorageMemberName, BindingFlags.NonPublic | BindingFlags.Instance);
                if ((member == null) || (member.Length != 1))
                {
                    throw Error.BadStorageProperty(this.memberMap.StorageMemberName, mi.DeclaringType, mi.Name);
                }
                this.storageMember = member[0];
            }
            Type clrType = (this.storageMember != null) ? TypeSystem.GetMemberType(this.storageMember) : this.type;
            this.isDeferred = this.IsDeferredType(clrType);
            ColumnMapping mapping = map as ColumnMapping;
            if ((((mapping != null) && mapping.IsDbGenerated) && (mapping.IsPrimaryKey && (mapping.AutoSync != AutoSync.Default))) && (mapping.AutoSync != AutoSync.OnInsert))
            {
                throw Error.IncorrectAutoSyncSpecification(mi.Name);
            }
            if (mapping != null)
            {
                this.isPrimaryKey = mapping.IsPrimaryKey;
                this.isVersion = mapping.IsVersion;
                this.isDBGenerated = (mapping.IsDbGenerated || !string.IsNullOrEmpty(mapping.Expression)) || this.isVersion;
                this.isDiscriminator = mapping.IsDiscriminator;
                this.canBeNull = !mapping.CanBeNull.HasValue ? (this.isNullableType || !this.type.IsValueType) : mapping.CanBeNull.Value;
                this.dbType = mapping.DbType;
                this.expression = mapping.Expression;
                this.updateCheck = mapping.UpdateCheck;
                if (this.IsDbGenerated && this.IsPrimaryKey)
                {
                    this.autoSync = AutoSync.OnInsert;
                }
                else if (mapping.AutoSync != AutoSync.Default)
                {
                    this.autoSync = mapping.AutoSync;
                }
                else if (this.IsDbGenerated)
                {
                    this.autoSync = AutoSync.Always;
                }
            }
            this.mappedName = (this.memberMap.DbName != null) ? this.memberMap.DbName : this.member.Name;
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

        public override bool IsDeclaredBy(MetaType metaType)
        {
            if (metaType == null)
            {
                throw Error.ArgumentNull("metaType");
            }
            return (metaType.Type == this.member.DeclaringType);
        }

        private bool IsDeferredType(Type clrType)
        {
            if ((clrType == null) || (clrType == typeof(object)))
            {
                return false;
            }
            if (!clrType.IsGenericType)
            {
                return false;
            }
            Type genericTypeDefinition = clrType.GetGenericTypeDefinition();
            if (((genericTypeDefinition != typeof(Link<>)) && !typeof(EntitySet<>).IsAssignableFrom(genericTypeDefinition)) && !typeof(EntityRef<>).IsAssignableFrom(genericTypeDefinition))
            {
                return this.IsDeferredType(clrType.BaseType);
            }
            return true;
        }

        private static void MakeDeferredAccessors(Type declaringType, MetaAccessor accessor, out MetaAccessor accessorValue, out MetaAccessor accessorDeferredValue, out MetaAccessor accessorDeferredSource)
        {
            if (accessor.Type.IsGenericType)
            {
                Type genericTypeDefinition = accessor.Type.GetGenericTypeDefinition();
                Type type2 = accessor.Type.GetGenericArguments()[0];
                if (genericTypeDefinition == typeof(Link<>))
                {
                    accessorValue = CreateAccessor(typeof(LinkValueAccessor<,>).MakeGenericType(new Type[] { declaringType, type2 }), new object[] { accessor });
                    accessorDeferredValue = CreateAccessor(typeof(LinkDefValueAccessor<,>).MakeGenericType(new Type[] { declaringType, type2 }), new object[] { accessor });
                    accessorDeferredSource = CreateAccessor(typeof(LinkDefSourceAccessor<,>).MakeGenericType(new Type[] { declaringType, type2 }), new object[] { accessor });
                    return;
                }
                if (typeof(EntityRef<>).IsAssignableFrom(genericTypeDefinition))
                {
                    accessorValue = CreateAccessor(typeof(EntityRefValueAccessor<,>).MakeGenericType(new Type[] { declaringType, type2 }), new object[] { accessor });
                    accessorDeferredValue = CreateAccessor(typeof(EntityRefDefValueAccessor<,>).MakeGenericType(new Type[] { declaringType, type2 }), new object[] { accessor });
                    accessorDeferredSource = CreateAccessor(typeof(EntityRefDefSourceAccessor<,>).MakeGenericType(new Type[] { declaringType, type2 }), new object[] { accessor });
                    return;
                }
                if (typeof(EntitySet<>).IsAssignableFrom(genericTypeDefinition))
                {
                    accessorValue = CreateAccessor(typeof(EntitySetValueAccessor<,>).MakeGenericType(new Type[] { declaringType, type2 }), new object[] { accessor });
                    accessorDeferredValue = CreateAccessor(typeof(EntitySetDefValueAccessor<,>).MakeGenericType(new Type[] { declaringType, type2 }), new object[] { accessor });
                    accessorDeferredSource = CreateAccessor(typeof(EntitySetDefSourceAccessor<,>).MakeGenericType(new Type[] { declaringType, type2 }), new object[] { accessor });
                    return;
                }
            }
            throw Error.UnhandledDeferredStorageType(accessor.Type);
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
                            this.assoc = new MappedAssociation(this, (AssociationMapping)this.memberMap);
                        }
                    }
                }
                return this.assoc;
            }
        }

        public override AutoSync AutoSync
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.autoSync;
            }
        }

        public override bool CanBeNull
        {
            get
            {
                return this.canBeNull;
            }
        }

        public override string DbType
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.dbType;
            }
        }

        public override MetaType DeclaringType
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.declaringType;
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
                return this.expression;
            }
        }

        public override bool IsAssociation
        {
            get
            {
                return (this.memberMap is AssociationMapping);
            }
        }

        public override bool IsDbGenerated
        {
            get
            {
                return this.isDBGenerated;
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
                return this.isDiscriminator;
            }
        }

        public override bool IsPersistent
        {
            get
            {
                return (this.memberMap != null);
            }
        }

        public override bool IsPrimaryKey
        {
            get
            {
                return this.isPrimaryKey;
            }
        }

        public override bool IsVersion
        {
            get
            {
                return this.isVersion;
            }
        }

        public override MethodInfo LoadMethod
        {
            get
            {
                if (!this.hasLoadMethod && this.IsDeferred)
                {
                    this.loadMethod = MethodFinder.FindMethod(((MappedMetaModel)this.declaringType.Model).ContextType, "Load" + this.member.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, new Type[] { this.DeclaringType.Type });
                    this.hasLoadMethod = true;
                }
                return this.loadMethod;
            }
        }

        public override string MappedName
        {
            get
            {
                return this.mappedName;
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
                return this.updateCheck;
            }
        }
    }

    internal class EntitySetDefSourceAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> where V : class
    {
        // Fields
        private MetaAccessor<T, EntitySet<V>> acc;

        // Methods
        internal EntitySetDefSourceAccessor(MetaAccessor<T, EntitySet<V>> acc)
        {
            this.acc = acc;
        }

        public override IEnumerable<V> GetValue(T instance)
        {
            return this.acc.GetValue(instance).Source;
        }

        public override void SetValue(ref T instance, IEnumerable<V> value)
        {
            EntitySet<V> set = this.acc.GetValue(instance);
            if (set == null)
            {
                set = new EntitySet<V>();
                this.acc.SetValue(ref instance, set);
            }
            set.SetSource(value);
        }
    }

 


    internal class EntitySetDefValueAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> where V : class
    {
        // Fields
        private MetaAccessor<T, EntitySet<V>> acc;

        // Methods
        internal EntitySetDefValueAccessor(MetaAccessor<T, EntitySet<V>> acc)
        {
            this.acc = acc;
        }

        public override IEnumerable<V> GetValue(T instance)
        {
            return this.acc.GetValue(instance).GetUnderlyingValues();
        }

        public override void SetValue(ref T instance, IEnumerable<V> value)
        {
            EntitySet<V> set = this.acc.GetValue(instance);
            if (set == null)
            {
                set = new EntitySet<V>();
                this.acc.SetValue(ref instance, set);
            }
            set.Assign(value);
        }
    }

 

}