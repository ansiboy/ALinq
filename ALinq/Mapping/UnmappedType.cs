using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal sealed class UnmappedType : MetaType
    {
        // Fields
        private static readonly ReadOnlyCollection<MetaAssociation> _emptyAssociations = new List<MetaAssociation>().AsReadOnly();
        private static readonly ReadOnlyCollection<MetaDataMember> _emptyDataMembers = new List<MetaDataMember>().AsReadOnly();
        private static readonly ReadOnlyCollection<MetaType> _emptyTypes = new List<MetaType>().AsReadOnly();
        private Dictionary<object, MetaDataMember> dataMemberMap;
        private ReadOnlyCollection<MetaDataMember> dataMembers;
        private ReadOnlyCollection<MetaType> inheritanceTypes;
        private readonly object locktarget = new object();
        private readonly MetaModel model;
        private readonly Type type;

        // Methods
        internal UnmappedType(MetaModel model, Type type)
        {
            this.model = model;
            this.type = type;
        }

        public override MetaDataMember GetDataMember(MemberInfo mi)
        {
            MetaDataMember member2;
            if (mi == null)
            {
                throw Error.ArgumentNull("mi");
            }
            InitDataMembers();
            if (dataMemberMap == null)
            {
                lock (locktarget)
                {
                    if (dataMemberMap == null)
                    {
                        var dictionary = new Dictionary<object, MetaDataMember>();
                        foreach (var member in dataMembers)
                        {
                            dictionary.Add(InheritanceRules.DistinguishedMemberName(member.Member), member);
                        }
                        dataMemberMap = dictionary;
                    }
                }
            }
            object key = InheritanceRules.DistinguishedMemberName(mi);
            dataMemberMap.TryGetValue(key, out member2);
            return member2;
        }

        public override MetaType GetInheritanceType(Type inheritanceType)
        {
            if (inheritanceType == type)
            {
                return this;
            }
            return null;
        }

        public override MetaType GetTypeForInheritanceCode(object key)
        {
            return null;
        }

        private void InitDataMembers()
        {
            if (dataMembers == null)
            {
                lock (locktarget)
                {
                    if (dataMembers == null)
                    {
                        var list = new List<MetaDataMember>();
                        int ordinal = 0;
                        const BindingFlags bindingAttr = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic |
                                                         BindingFlags.Public | BindingFlags.Instance;
                        foreach (var info in type.GetFields(bindingAttr))
                        {
                            MetaDataMember item = new UnmappedDataMember(this, info, ordinal);
                            list.Add(item);
                            ordinal++;
                        }
                        foreach (var info2 in type.GetProperties(bindingAttr))
                        {
                            MetaDataMember member2 = new UnmappedDataMember(this, info2, ordinal);
                            list.Add(member2);
                            ordinal++;
                        }
                        dataMembers = list.AsReadOnly();
                    }
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        // Properties
        public override ReadOnlyCollection<MetaAssociation> Associations
        {
            get
            {
                return _emptyAssociations;
            }
        }

        public override bool CanInstantiate
        {
            get
            {
                return !type.IsAbstract;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> DataMembers
        {
            get
            {
                InitDataMembers();
                return dataMembers;
            }
        }

        public override MetaDataMember DBGeneratedIdentityMember
        {
            get
            {
                return null;
            }
        }

        public override ReadOnlyCollection<MetaType> DerivedTypes
        {
            get
            {
                return _emptyTypes;
            }
        }

        public override MetaDataMember Discriminator
        {
            get
            {
                return null;
            }
        }

        public override bool HasAnyLoadMethod
        {
            get
            {
                return false;
            }
        }

        public override bool HasAnyValidateMethod
        {
            get
            {
                return false;
            }
        }

        internal override bool HasAnySetDataContextMethod
        {
            get { return false; ; }
        }

        public override bool HasInheritance
        {
            get
            {
                return false;
            }
        }

        public override bool HasInheritanceCode
        {
            get
            {
                return false;
            }
        }

        public override bool HasUpdateCheck
        {
            get
            {
                return false;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> IdentityMembers
        {
            get
            {
                InitDataMembers();
                return dataMembers;
            }
        }

        public override MetaType InheritanceBase
        {
            get
            {
                return null;
            }
        }

        public override object InheritanceCode
        {
            get
            {
                return null;
            }
        }

        public override MetaType InheritanceDefault
        {
            get
            {
                return null;
            }
        }

        public override MetaType InheritanceRoot
        {
            get
            {
                return this;
            }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes
        {
            get
            {
                if (this.inheritanceTypes == null)
                {
                    lock (this.locktarget)
                    {
                        if (this.inheritanceTypes == null)
                        {
                            this.inheritanceTypes = new MetaType[] { this }.ToList<MetaType>().AsReadOnly();
                        }
                    }
                }
                return this.inheritanceTypes;
            }
        }

        public override bool IsEntity
        {
            get
            {
                return false;
            }
        }

        public override bool IsInheritanceDefault
        {
            get
            {
                return false;
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
                return null;
            }
        }

        public override MethodInfo OnValidateMethod
        {
            get
            {
                return null;
            }
        }

        internal override MethodInfo SetDataContextMehod
        {
            get { return null; }
        }

        public override ReadOnlyCollection<MetaDataMember> PersistentDataMembers
        {
            get
            {
                return _emptyDataMembers;
            }
        }

        public override MetaTable Table
        {
            get
            {
                return null;
            }
        }

        public override Type Type
        {
            get
            {
                return type;
            }
        }

        public override MetaDataMember VersionMember
        {
            get
            {
                return null;
            }
        }
    }
}