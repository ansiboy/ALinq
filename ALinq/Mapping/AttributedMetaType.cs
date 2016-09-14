using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ALinq.SqlClient;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
    internal class AttributedMetaType : MetaType
    {
        // Fields
        private ReadOnlyCollection<MetaAssociation> associations;
        private Dictionary<MetaPosition, MetaDataMember> dataMemberMap;
        private ReadOnlyCollection<MetaDataMember> dataMembers;
        private MetaDataMember dbGeneratedIdentity;
        private ReadOnlyCollection<MetaType> derivedTypes;
        private MetaDataMember discriminator;
        private bool hasAnyLoadMethod;
        private bool hasAnyValidateMethod;
        private bool hasMethods;
        private ReadOnlyCollection<MetaDataMember> identities;
        private MetaType inheritanceBase;
        private bool inheritanceBaseSet;
        internal object inheritanceCode;
        private MetaType inheritanceRoot;
        private object locktarget = new object();
        private MetaModel model;
        private MethodInfo onLoadedMethod;
        private MethodInfo onValidateMethod;
        private ReadOnlyCollection<MetaDataMember> persistentMembers;
        private MetaTable table;
        private Type type;
        private MetaDataMember version;
        private MethodInfo setDataContextMethod;
        private bool hasAnySetDataContextMethod;

        // Methods
        internal AttributedMetaType(MetaModel model, MetaTable table, Type type, MetaType inheritanceRoot)
        {
            this.model = model;
            this.table = table;
            this.type = type;
            this.inheritanceRoot = inheritanceRoot ?? this;
            InitDataMembers();
            identities = dataMembers.Where(m => m.IsPrimaryKey).ToList().AsReadOnly();
            persistentMembers = dataMembers.Where(m => m.IsPersistent).ToList().AsReadOnly();
        }

        public override MetaDataMember GetDataMember(MemberInfo mi)
        {
            if (mi == null)
            {
                throw ALinq.Error.ArgumentNull("mi");
            }
            MetaDataMember member = null;

            //ALinq 2.6.3 添加功能，实体类通过继承 Interface 接口，通过接口可以进行查询。
            if (mi.DeclaringType.IsInterface)
            {
                var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                         BindingFlags.GetField | BindingFlags.GetProperty;
                var m = Type.GetMember(mi.Name, bf);
                if (m.Length > 0)
                    mi = m[0];
            }
            //===================================================

            if (this.dataMemberMap.TryGetValue(new MetaPosition(mi), out member))
            {
                return member;
            }
            if (mi.DeclaringType.IsInterface)
            {
                throw Mapping.Error.MappingOfInterfacesMemberIsNotSupported(mi.DeclaringType.Name, mi.Name);
            }
            throw Mapping.Error.UnmappedClassMember(mi.DeclaringType.Name, mi.Name);
        }

        public override MetaType GetInheritanceType(Type inheritanceType)
        {
            if (inheritanceType == this.type)
            {
                return this;
            }
            return this.inheritanceRoot.GetInheritanceType(inheritanceType);
        }

        public override MetaType GetTypeForInheritanceCode(object key)
        {
            if (this.InheritanceRoot.Discriminator.Type == typeof(string))
            {
                string strB = (string)key;
                foreach (MetaType type in this.InheritanceRoot.InheritanceTypes)
                {
                    if (string.Compare((string)type.InheritanceCode, strB, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return type;
                    }
                }
            }
            else
            {
                foreach (MetaType type2 in this.InheritanceRoot.InheritanceTypes)
                {
                    if (object.Equals(type2.InheritanceCode, key))
                    {
                        return type2;
                    }
                }
            }
            return null;
        }

        private void InitDataMembers()
        {
            if (this.dataMembers == null)
            {
                this.dataMemberMap = new Dictionary<MetaPosition, MetaDataMember>();
                int ordinal = 0;
                BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                FieldInfo[] infoArray = TypeSystem.GetAllFields(this.type, flags).ToArray<FieldInfo>();
                if (infoArray != null)
                {
                    int index = 0;
                    int length = infoArray.Length;
                    while (index < length)
                    {
                        FieldInfo mi = infoArray[index];
                        MetaDataMember mm = new AttributedMetaDataMember(this, mi, ordinal);
                        this.ValidatePrimaryKeyMember(mm);
                        if (mm.IsPersistent || mi.IsPublic)
                        {
                            this.dataMemberMap.Add(new MetaPosition(mi), mm);
                            ordinal++;
                            if (mm.IsPersistent)
                            {
                                this.InitSpecialMember(mm);
                            }
                        }
                        index++;
                    }
                }
                PropertyInfo[] infoArray2 = TypeSystem.GetAllProperties(this.type, flags).ToArray<PropertyInfo>();
                if (infoArray2 != null)
                {
                    int num4 = 0;
                    int num5 = infoArray2.Length;
                    while (num4 < num5)
                    {
                        PropertyInfo info2 = infoArray2[num4];
                        MetaDataMember member2 = new AttributedMetaDataMember(this, info2, ordinal);
                        this.ValidatePrimaryKeyMember(member2);
                        bool flag = (info2.CanRead && (info2.GetGetMethod(false) != null)) && (!info2.CanWrite || (info2.GetSetMethod(false) != null));
                        if (member2.IsPersistent || flag)
                        {
                            this.dataMemberMap.Add(new MetaPosition(info2), member2);
                            ordinal++;
                            if (member2.IsPersistent)
                            {
                                this.InitSpecialMember(member2);
                            }
                        }
                        num4++;
                    }
                }
                this.dataMembers = new List<MetaDataMember>(this.dataMemberMap.Values).AsReadOnly();
            }
        }

        private void InitMethods()
        {
            if (!this.hasMethods)
            {
                var bf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                //BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
                this.onLoadedMethod = MethodFinder.FindMethod(this.Type, "OnLoaded", bf, Type.EmptyTypes, false);
                this.onValidateMethod = MethodFinder.FindMethod(this.Type, "OnValidate", bf, new Type[] { typeof(ChangeAction) }, false);
                this.setDataContextMethod = MethodFinder.FindMethod(this.Type, "SetDataContext", bf, new Type[] { typeof(DataContext) }, false);
                this.hasAnyLoadMethod = (this.onLoadedMethod != null) || ((this.InheritanceBase != null) && this.InheritanceBase.HasAnyLoadMethod);
                this.hasAnyValidateMethod = (this.onValidateMethod != null) || ((this.InheritanceBase != null) && this.InheritanceBase.HasAnyValidateMethod);
                this.hasAnySetDataContextMethod = (this.setDataContextMethod != null) || ((this.InheritanceBase != null) && this.InheritanceBase.HasAnySetDataContextMethod);
                this.hasMethods = true;
            }
        }

        private void InitSpecialMember(MetaDataMember mm)
        {
            if ((mm.IsDbGenerated && mm.IsPrimaryKey) && string.IsNullOrEmpty(mm.Expression))
            {
                if (this.dbGeneratedIdentity != null)
                {
                    throw Mapping.Error.TwoMembersMarkedAsPrimaryKeyAndDBGenerated(mm.Member, this.dbGeneratedIdentity.Member);
                }
                this.dbGeneratedIdentity = mm;
            }
            if (mm.IsPrimaryKey && !MappingSystem.IsSupportedIdentityType(mm.Type))
            {
                throw Mapping.Error.IdentityClrTypeNotSupported(mm.DeclaringType, mm.Name, mm.Type);
            }
            if (mm.IsVersion)
            {
                if (this.version != null)
                {
                    throw Mapping.Error.TwoMembersMarkedAsRowVersion(mm.Member, this.version.Member);
                }
                this.version = mm;
            }
            if (mm.IsDiscriminator)
            {
                if (this.discriminator != null)
                {
                    throw Mapping.Error.TwoMembersMarkedAsInheritanceDiscriminator(mm.Member, this.discriminator.Member);
                }
                this.discriminator = mm;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        private void ValidatePrimaryKeyMember(MetaDataMember mm)
        {
            if ((mm.IsPrimaryKey && (this.inheritanceRoot != this)) && (mm.Member.DeclaringType == this.type))
            {
                throw Mapping.Error.PrimaryKeyInSubTypeNotSupported(this.type.Name, mm.Name);
            }
        }

        // Properties
        public override ReadOnlyCollection<MetaAssociation> Associations
        {
            get
            {
                if (this.associations == null)
                {
                    lock (this.locktarget)
                    {
                        if (this.associations == null)
                        {
                            this.associations = this.dataMembers.Where(delegate(MetaDataMember m)
                            {
                                return m.IsAssociation;
                            }).Select(delegate(MetaDataMember m)
                            {
                                return m.Association;
                            }).ToList().AsReadOnly();
                        }
                    }
                }
                return this.associations;
            }
        }

        public override bool CanInstantiate
        {
            get
            {
                if (this.type.IsAbstract)
                {
                    return false;
                }
                if (this != this.InheritanceRoot)
                {
                    return this.HasInheritanceCode;
                }
                return true;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> DataMembers
        {
            get
            {
                return this.dataMembers;
            }
        }

        public override MetaDataMember DBGeneratedIdentityMember
        {
            get
            {
                return this.dbGeneratedIdentity;
            }
        }

        public override ReadOnlyCollection<MetaType> DerivedTypes
        {
            get
            {
                if (this.derivedTypes == null)
                {
                    lock (this.locktarget)
                    {
                        if (this.derivedTypes == null)
                        {
                            List<MetaType> list = new List<MetaType>();
                            foreach (MetaType type in this.InheritanceTypes)
                            {
                                if (type.Type.BaseType == this.type)
                                {
                                    list.Add(type);
                                }
                            }
                            this.derivedTypes = list.AsReadOnly();
                        }
                    }
                }
                return this.derivedTypes;
            }
        }

        public override MetaDataMember Discriminator
        {
            get
            {
                return this.discriminator;
            }
        }

        public override bool HasAnyLoadMethod
        {
            get
            {
                this.InitMethods();
                return this.hasAnyLoadMethod;
            }
        }

        public override bool HasAnyValidateMethod
        {
            get
            {
                this.InitMethods();
                return this.hasAnyValidateMethod;
            }
        }

        internal override bool HasAnySetDataContextMethod
        {
            get
            {
                this.InitMethods();
                return this.hasAnySetDataContextMethod;
            }
        }

        public override bool HasInheritance
        {
            get
            {
                return this.inheritanceRoot.HasInheritance;
            }
        }

        public override bool HasInheritanceCode
        {
            get
            {
                return (this.inheritanceCode != null);
            }
        }

        public override bool HasUpdateCheck
        {
            get
            {
                foreach (MetaDataMember member in this.PersistentDataMembers)
                {
                    if (member.UpdateCheck != UpdateCheck.Never)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> IdentityMembers
        {
            get
            {
                return this.identities;
            }
        }

        public override MetaType InheritanceBase
        {
            get
            {
                if (!this.inheritanceBaseSet && (this.inheritanceBase == null))
                {
                    lock (this.locktarget)
                    {
                        if (this.inheritanceBase == null)
                        {
                            this.inheritanceBase = InheritanceBaseFinder.FindBase(this);
                            this.inheritanceBaseSet = true;
                        }
                    }
                }
                return this.inheritanceBase;
            }
        }

        public override object InheritanceCode
        {
            get
            {
                return this.inheritanceCode;
            }
        }

        public override MetaType InheritanceDefault
        {
            get
            {
                return this.InheritanceRoot.InheritanceDefault;
            }
        }

        public override MetaType InheritanceRoot
        {
            get
            {
                return this.inheritanceRoot;
            }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes
        {
            get
            {
                return this.inheritanceRoot.InheritanceTypes;
            }
        }

        public override bool IsEntity
        {
            get
            {
                return (this.table != null && this.table.RowType.IdentityMembers.Count > 0);
            }
        }

        public override bool IsInheritanceDefault
        {
            get
            {
                return (this.InheritanceDefault == this);
            }
        }

        public override MetaModel Model
        {
            get
            {
                return this.model;
            }
        }

        public override string Name
        {
            get
            {
                return this.type.Name;
            }
        }

        public override MethodInfo OnLoadedMethod
        {
            get
            {
                this.InitMethods();
                return this.onLoadedMethod;
            }
        }

        public override MethodInfo OnValidateMethod
        {
            get
            {
                this.InitMethods();
                return this.onValidateMethod;
            }
        }

        internal override MethodInfo SetDataContextMehod
        {
            get { return this.setDataContextMethod; }
        }

        public override ReadOnlyCollection<MetaDataMember> PersistentDataMembers
        {
            get
            {
                return this.persistentMembers;
            }
        }

        public override MetaTable Table
        {
            get
            {
                return this.table;
            }
        }

        public override Type Type
        {
            get
            {
                return this.type;
            }
        }

        public override MetaDataMember VersionMember
        {
            get
            {
                return this.version;
            }
        }
    }
}