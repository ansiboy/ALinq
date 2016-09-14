using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal class DynamicMetaType : MetaType
    {
        #region Sub Class MyList
        class MyList : IList<MetaDataMember>
        {
            class MyListEnumerator : IEnumerator<MetaDataMember>
            {
                private List<MetaDataMember>.Enumerator e1;
                private Dictionary<string, MetaDataMember>.ValueCollection.Enumerator e2;
                private MyList mylist;
                private MetaDataMember current;

                public MyListEnumerator(MyList mylist)
                {
                    this.mylist = mylist;
                    e1 = mylist.sourceItems.GetEnumerator();
                    e2 = mylist.extMembers.Values.GetEnumerator();
                }

                public bool MoveNext()
                {
                    if (e1.MoveNext())
                    {
                        this.current = e1.Current;
                        return true;
                    }

                    if (e2.MoveNext())
                    {
                        this.current = e2.Current;
                        return true;
                    }

                    this.current = null;
                    return false;
                }

                void IEnumerator.Reset()
                {
                    ((IEnumerator)e1).Reset();
                    ((IEnumerator)e2).Reset();
                }

                public MetaDataMember Current
                {
                    get
                    {
                        return current;
                    }
                }

                object IEnumerator.Current
                {
                    get { return this.Current; }
                }

                public void Dispose()
                {
                    e1.Dispose();
                    e2.Dispose();
                }
            }

            //private DynamicMetaType metaType;
            private List<MetaDataMember> sourceItems;
            //private MyListEnumerator enumerator;
            //private MetaDataMember indexerDataMember;
            private Dictionary<string, MetaDataMember> extMembers;

            public MyList(IList<MetaDataMember> dataMembers, Dictionary<string, MetaDataMember> extMembers)
            {
                this.extMembers = extMembers;
                sourceItems = new List<MetaDataMember>();
                for (var i = 0; i < dataMembers.Count; i++)
                {
                    var dataMember = dataMembers[i];
                    if (dataMember.Name == "Item")
                    {
                        //this.indexerDataMember = dataMember;
                        continue;
                    }
                    sourceItems.Add(dataMember);
                }
                //this.enumerator = new MyListEnumerator(this);
            }

            public IEnumerator<MetaDataMember> GetEnumerator()
            {
                return new MyListEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(MetaDataMember item)
            {
                extMembers.Add(item.Name, item);
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(MetaDataMember item)
            {
                if (item.Member is IndexerMemberInfo)
                    return extMembers.Values.Contains(item);

                return sourceItems.Contains(item);
            }

            public void CopyTo(MetaDataMember[] array, int arrayIndex)
            {
                var q = this.extMembers.Values.ToArray();
                var items = sourceItems.Union(this.extMembers.Values.Cast<MetaDataMember>()).ToArray();
                int count = 0;
                for (var i = arrayIndex; i < items.Length; i++)
                {
                    array[count] = items[i];
                    count = count + 1;
                }
            }

            public bool Remove(MetaDataMember item)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get
                {
                    return sourceItems.Count + extMembers.Count;
                }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            //public MetaDataMember IndexerDataMember
            //{
            //    get { return this.indexerDataMember; }
            //}

            public int IndexOf(MetaDataMember item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, MetaDataMember item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public MetaDataMember this[int index]
            {
                get
                {
                    if (index < sourceItems.Count)
                        return sourceItems[index];

                    index = index - sourceItems.Count;
                    return extMembers.Values.Where((o, i) => i == index).SingleOrDefault();
                }
                set { throw new NotImplementedException(); }
            }
        } 
        #endregion

        private MetaType source;
        private DynamicModel model;
        private DynamicRootType rootType;
        public static int MaxDataMembersCount = 300;
        private Dictionary<string, MetaDataMember> extendDataMembers;
        private ReadOnlyCollection<MetaDataMember> dataMembers;
        private ReadOnlyCollection<MetaDataMember> persistentDataMembers;
        private MyList dataMemberList;
        private Dictionary<string, MetaDataMember> extendPersistentDataMembers;
        private ReadOnlyCollection<MetaDataMember> identities;

        internal DynamicMetaType(DynamicModel model, MetaType source)
        {
            this.source = source;
            this.model = model;
            this.extendDataMembers = new Dictionary<string, MetaDataMember>();
            this.extendPersistentDataMembers = new Dictionary<string, MetaDataMember>();

            dataMemberList = new MyList(source.DataMembers, extendDataMembers);
            var mylist2 = new MyList(source.PersistentDataMembers, extendPersistentDataMembers);
            this.dataMembers = new ReadOnlyCollection<MetaDataMember>(dataMemberList);
            this.persistentDataMembers = new ReadOnlyCollection<MetaDataMember>(mylist2);
            if (model.Source is AttributedMetaModel)
            {
                if (((AttributedMetaModel)model.Source).AttributeProvider is FluentMappingSource.AttributeProvider)
                {
                    var attrProvider =
                        ((FluentMappingSource.AttributeProvider)((AttributedMetaModel)model.Source).AttributeProvider);

                    var entityMapping = attrProvider.Mapping.GetEntityMapping(source.Type);
                    if (entityMapping != null)
                    {
                        foreach (var item in entityMapping.MemberColumnPairs)
                        {
                            if (item.Key is IndexerMemberInfo)
                            {
                                var dataMember = new AttributedMetaDataMember((AttributedMetaType)this.Source, item.Key, dataMembers.Count);
                                extendDataMembers[item.Key.Name] = dataMember;
                                extendPersistentDataMembers[item.Key.Name] = dataMember;
                            }
                        }
                    }

                }
            }

            this.identities = dataMembers.Where(m => m.IsPrimaryKey).ToList().AsReadOnly();
        }

        public DynamicRootType RootType
        {
            get { return (DynamicRootType)this.InheritanceRoot; }
        }

        public override MetaDataMember GetDataMember(MemberInfo member)
        {
            if (member is IndexerMemberInfo)
            {
                MetaDataMember item;
                if (RootType.ExtendDataMembers.TryGetValue(member.Name, out item))
                    return item;

                throw Mapping.Error.UnmappedClassMember(member.DeclaringType.Name, member.Name);
            }
            else
            {
                return source.GetDataMember(member);
            }
        }

        public override MetaType GetInheritanceType(Type type)
        {
            var metaType = source.GetInheritanceType(type);
            return model.GetMetaTypeBySource(metaType);
        }

        public override MetaType GetTypeForInheritanceCode(object code)
        {
            var metaType = source.GetTypeForInheritanceCode(code);
            return model.GetMetaTypeBySource(metaType);
        }

        public override ReadOnlyCollection<MetaAssociation> Associations
        {
            get { return source.Associations; }
        }

        public override bool CanInstantiate
        {
            get
            {
                return source.CanInstantiate;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> DataMembers
        {
            get
            {
                return dataMembers;
            }
        }

        public override MetaDataMember DBGeneratedIdentityMember
        {
            get { return source.DBGeneratedIdentityMember; }
        }

        public override ReadOnlyCollection<MetaType> DerivedTypes
        {
            get
            {
                return source.DerivedTypes;
            }
        }

        public override MetaDataMember Discriminator
        {
            get { return source.Discriminator; }
        }

        public override bool HasAnyLoadMethod
        {
            get { return source.HasAnyLoadMethod; }
        }

        public override bool HasAnyValidateMethod
        {
            get { return source.HasAnyValidateMethod; }
        }

        internal override bool HasAnySetDataContextMethod
        {
            get { return source.HasAnySetDataContextMethod; }
        }

        public override bool HasInheritance
        {
            get { return source.HasInheritance; }
        }

        public override bool HasInheritanceCode
        {
            get { return source.HasInheritanceCode; }
        }

        public override bool HasUpdateCheck
        {
            get { return source.HasUpdateCheck; }
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
                var metaType = source.InheritanceBase;
                return model.GetMetaTypeBySource(metaType);
            }
        }

        public override object InheritanceCode
        {
            get { return source.InheritanceCode; }
        }

        public override MetaType InheritanceDefault
        {
            get
            {
                var metaType = source.InheritanceDefault;
                return model.GetMetaTypeBySource(metaType);
            }
        }

        public override MetaType InheritanceRoot
        {
            get
            {
                var metaType = source.InheritanceRoot;
                return model.GetMetaTypeBySource(metaType);
            }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes
        {
            get
            {
                var q = source.InheritanceTypes.Select(o => model.GetMetaTypeBySource(o));
                return new ReadOnlyCollection<MetaType>(q.ToArray());
            }
        }

        public override bool IsEntity
        {
            get
            {
                return (source.Table != null && IdentityMembers.Count > 0);
            }
        }

        public override bool IsInheritanceDefault
        {
            get { return source.IsInheritanceDefault; }
        }

        public override MetaModel Model
        {
            get { return this.model; }
        }

        public override string Name
        {
            get { return source.Name; }
        }

        public override MethodInfo OnLoadedMethod
        {
            get { return source.OnLoadedMethod; }
        }

        public override MethodInfo OnValidateMethod
        {
            get { return source.OnValidateMethod; }
        }

        internal override MethodInfo SetDataContextMehod
        {
            get { return source.SetDataContextMehod; }
        }

        public override ReadOnlyCollection<MetaDataMember> PersistentDataMembers
        {
            get
            {
                return this.persistentDataMembers;
            }
        }

        public override MetaTable Table
        {
            get
            {
                var metaTable = source.Table;
                var result = model.GetMetaTableBySource(metaTable);
                return result;
            }
        }

        public override Type Type
        {
            get { return source.Type; }
        }

        public override MetaDataMember VersionMember
        {
            get { return source.VersionMember; }
        }

        public MetaType Source
        {
            get { return this.source; }
        }

        public Dictionary<string, MetaDataMember> ExtendDataMembers
        {
            get
            {
                return extendDataMembers;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is DynamicMetaType)
                return source == ((DynamicMetaType)obj).source;

            return base.Equals(obj);
        }

    }

}