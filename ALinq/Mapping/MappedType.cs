using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ALinq.SqlClient;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
    internal class MappedType : MetaType
    {
        private const string OnLoadedMethodName = "OnLoaded";
        private const string OnValidateMethodName = "OnValidate";
        private const string SetDataContextMethodName = "SetDataContext";

        // Fields
        private ReadOnlyCollection<MetaAssociation> associations;
        private Dictionary<object, MetaDataMember> dataMemberMap;
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
        private readonly ReadOnlyCollection<MetaDataMember> persistentDataMembers;
        private readonly MetaTable table;
        private readonly Type type;
        private readonly TypeMapping typeMapping;
        private MetaDataMember version;
        private MethodInfo setDataContextMethod;
        private bool hasAnySetDataContextMethod;

        // Methods
        internal MappedType(MetaModel model, MetaTable table, TypeMapping typeMapping, Type type, MetaType inheritanceRoot)
        {
            this.model = model;
            this.table = table;
            this.typeMapping = typeMapping;
            this.type = type;
            this.inheritanceRoot = inheritanceRoot ?? this;
            this.InitDataMembers();
            this.identities = this.dataMembers.Where(m => m.IsPrimaryKey).ToList().AsReadOnly();
            this.persistentDataMembers = this.dataMembers.Where(m => m.IsPersistent).ToList().AsReadOnly();
        }

        public override MetaDataMember GetDataMember(MemberInfo mi)
        {
            MetaDataMember member;
            if (mi == null)
            {
                throw Error.ArgumentNull("mi");
            }
            if (this.dataMemberMap.TryGetValue(InheritanceRules.DistinguishedMemberName(mi), out member))
            {
                return member;
            }
            if (mi.DeclaringType.IsInterface)
            {
                throw Error.MappingOfInterfacesMemberIsNotSupported(mi.DeclaringType.Name, mi.Name);
            }
            throw Error.UnmappedClassMember(mi.DeclaringType.Name, mi.Name);
        }

        public override MetaType GetInheritanceType(Type inheritanceType)
        {
            foreach (MetaType type in this.InheritanceTypes)
            {
                if (type.Type == inheritanceType)
                {
                    return type;
                }
            }
            return null;
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
            if (dataMembers == null)
            {
                var dictionary = new Dictionary<object, MetaDataMember>();
                var list = new List<MetaDataMember>();
                int ordinal = 0;
                const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                var dictionary2 = new Dictionary<string, MemberMapping>();
                Type baseType = type;
                for (TypeMapping mapping = this.typeMapping; mapping != null; mapping = mapping.BaseType)
                {
                    foreach (MemberMapping mapping2 in mapping.Members)
                    {
                        dictionary2[mapping2.MemberName + ":" + baseType.Name] = mapping2;
                    }
                    baseType = baseType.BaseType;
                }
                var set = new HashSet<string>();
                FieldInfo[] infoArray = TypeSystem.GetAllFields(this.type, flags).ToArray<FieldInfo>();
                if (infoArray != null)
                {
                    foreach (FieldInfo info in infoArray)
                    {
                        MemberMapping mapping3;
                        string key = info.Name + ":" + info.DeclaringType.Name;
                        if (dictionary2.TryGetValue(key, out mapping3))
                        {
                            MetaDataMember member;
                            set.Add(key);
                            object obj2 = InheritanceRules.DistinguishedMemberName(info);
                            if (!dictionary.TryGetValue(obj2, out member))
                            {
                                member = new MappedDataMember(this, info, mapping3, ordinal);
                                dictionary.Add(InheritanceRules.DistinguishedMemberName(member.Member), member);
                                list.Add(member);
                                this.InitSpecialMember(member);
                            }
                            this.ValidatePrimaryKeyMember(member);
                            ordinal++;
                        }
                    }
                }
                PropertyInfo[] infoArray2 = TypeSystem.GetAllProperties(this.type, flags).ToArray<PropertyInfo>();
                if (infoArray2 != null)
                {
                    foreach (PropertyInfo info2 in infoArray2)
                    {
                        MemberMapping mapping4;
                        string str2 = info2.Name + ":" + info2.DeclaringType.Name;
                        if (dictionary2.TryGetValue(str2, out mapping4))
                        {
                            MetaDataMember member2;
                            set.Add(str2);
                            object obj3 = InheritanceRules.DistinguishedMemberName(info2);
                            if (!dictionary.TryGetValue(obj3, out member2))
                            {
                                member2 = new MappedDataMember(this, info2, mapping4, ordinal);
                                dictionary.Add(InheritanceRules.DistinguishedMemberName(member2.Member), member2);
                                list.Add(member2);
                                this.InitSpecialMember(member2);
                            }
                            this.ValidatePrimaryKeyMember(member2);
                            ordinal++;
                        }
                    }
                }
                this.dataMembers = list.AsReadOnly();
                this.dataMemberMap = dictionary;
                foreach (string str3 in set)
                {
                    dictionary2.Remove(str3);
                }
                foreach (KeyValuePair<string, MemberMapping> pair in dictionary2)
                {
                    for (Type type2 = this.inheritanceRoot.Type.BaseType; type2 != null; type2 = type2.BaseType)
                    {
                        foreach (MemberInfo info3 in type2.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (string.Compare(info3.Name, pair.Value.MemberName, StringComparison.Ordinal) == 0)
                            {
                                throw Error.MappedMemberHadNoCorrespondingMemberInType(pair.Value.MemberName, this.type.Name);
                            }
                        }
                    }
                }
            }
        }

        private void InitMethods()
        {
            if (!this.hasMethods)
            {
                var bf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                // BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                this.onLoadedMethod = MethodFinder.FindMethod(this.Type, OnLoadedMethodName, bf, Type.EmptyTypes, false);
                this.onValidateMethod = MethodFinder.FindMethod(this.Type, OnValidateMethodName, bf, new Type[] { typeof(ChangeAction) }, false);
                this.setDataContextMethod = MethodFinder.FindMethod(this.Type, SetDataContextMethodName, bf,
                                                                    new Type[] { typeof(DataContext) }, false);
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
                    throw Error.TwoMembersMarkedAsPrimaryKeyAndDBGenerated(mm.Member, this.dbGeneratedIdentity.Member);
                }
                this.dbGeneratedIdentity = mm;
            }
            if (mm.IsPrimaryKey && !MappingSystem.IsSupportedIdentityType(mm.Type))
            {
                throw Error.IdentityClrTypeNotSupported(mm.DeclaringType, mm.Name, mm.Type);
            }
            if (mm.IsVersion)
            {
                if (this.version != null)
                {
                    throw Error.TwoMembersMarkedAsRowVersion(mm.Member, this.version.Member);
                }
                this.version = mm;
            }
            if (mm.IsDiscriminator)
            {
                if (this.discriminator != null)
                {
                    if (!InheritanceRules.AreSameMember(this.discriminator.Member, mm.Member))
                    {
                        throw Error.TwoMembersMarkedAsInheritanceDiscriminator(mm.Member, this.discriminator.Member);
                    }
                }
                else
                {
                    this.discriminator = mm;
                }
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
                throw Error.PrimaryKeyInSubTypeNotSupported(this.type.Name, mm.Name);
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
                            this.associations = this.dataMembers.Where<MetaDataMember>(delegate(MetaDataMember m)
                            {
                                return m.IsAssociation;
                            }).Select<MetaDataMember, MetaAssociation>(delegate(MetaDataMember m)
                            {
                                return m.Association;
                            }).ToList<MetaAssociation>().AsReadOnly();
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
                            var list = new List<MetaType>();
                            foreach (MetaType metaType in this.InheritanceTypes)
                            {
                                if (metaType.Type.BaseType == this.type)
                                {
                                    list.Add(metaType);
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
            get { return this.hasAnySetDataContextMethod; }
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
                return (this.InheritanceCode != null);
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
                if (this.inheritanceRoot == this)
                {
                    throw Error.CannotGetInheritanceDefaultFromNonInheritanceClass();
                }
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
                return ((this.table != null) && (this.table.RowType.IdentityMembers.Count > 0));
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
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.model;
            }
        }

        public override string Name
        {
            [System.Diagnostics.DebuggerStepThrough]
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
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.persistentDataMembers;
            }
        }

        public override MetaTable Table
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.table;
            }
        }

        public override Type Type
        {
            [System.Diagnostics.DebuggerStepThrough]
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