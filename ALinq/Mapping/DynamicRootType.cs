using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal class DynamicRootType : DynamicMetaType
    {
     

        //private MergeMetaTable table;
        private MetaType source;
        private DynamicModel model;

        private bool isRoot;
        private int ORDINAL_START = 0;

        internal DynamicRootType(DynamicModel model, MetaType source)
            : base(model, source)
        {
            Debug.Assert(source == source.InheritanceRoot);

            this.source = source;
            this.model = model;
            //indexerDataMember = source.DataMembers.FirstOrDefault(DynamicMappingSource.ItemMemberPredicate);

            ORDINAL_START = source.DataMembers.Count - 1;
        }

        //public override ReadOnlyCollection<MetaDataMember> PersistentDataMembers
        //{
        //    get
        //    {
        //        return base.PersistentDataMembers;
        //    }
        //}


        //public void AddOrUpdateDataMembers(DynamicRootType metaType, IndexerMemberInfo mf)
        //{
        //    if (source.DataMembers.FirstOrDefault(o => o.Name == mf.Name) != null)
        //        return;

        //    DynamicMetaDataMember member;
        //    int ordinal = ORDINAL_START + ExtendDataMembers.Count;
        //    if (ExtendDataMembers.TryGetValue(mf.Name, out member))
        //        ordinal = member.Ordinal;

        //    ExtendDataMembers[mf.Name] = new DynamicMetaDataMember(metaType, mf, ordinal);

        //    Debug.Assert(ExtendDataMembers.Values.Count(o => o.Ordinal == ordinal) == 1);
        //}

        public override bool Equals(object obj)
        {
            if (obj is DynamicRootType)
                return source == ((DynamicRootType)obj).source;

            return base.Equals(obj);
        }

    }
}