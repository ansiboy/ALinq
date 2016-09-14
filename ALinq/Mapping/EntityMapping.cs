using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ALinq.Mapping
{
    internal class EntityMapping
    {
        private Dictionary<MemberInfo, ColumnAttribute> columns = new Dictionary<MemberInfo, ColumnAttribute>();
        private Dictionary<MemberInfo, AssociationAttribute> associations = new Dictionary<MemberInfo, AssociationAttribute>();
      
        internal EntityMapping(Type entityType)
        {
            this.EntityType = entityType;
            InheritanceMappingAttribute = new List<InheritanceMappingAttribute>();
        }

        public Type EntityType { get; private set; }

        public TableAttribute TableAttribute { get; set; }

        public List<InheritanceMappingAttribute> InheritanceMappingAttribute { get; set; }

        public void Add(MemberInfo mi, ColumnAttribute column)
        {
            columns[mi] = column;
        }

        public void Add(MemberInfo mi, AssociationAttribute association)
        {
            associations[mi] = association;
        }

        public ColumnAttribute GetColumn(MemberInfo mi)
        {
            ColumnAttribute column;
            if (columns.TryGetValue(mi, out column))
                return column;

            return null;
        }

        public AssociationAttribute GetAssociation(MemberInfo mi)
        {
            AssociationAttribute association;
            if (associations.TryGetValue(mi, out association))
                return association;

            return null;
        }

        internal Dictionary<MemberInfo, ColumnAttribute> MemberColumnPairs
        {
            get { return this.columns; }
        }

        public IEnumerable<ColumnAttribute> Columns
        {
            get { return this.columns.Values; }
        }

        public IEnumerable<AssociationAttribute> Associations
        {
            get { return this.associations.Values; }
        }
    }

    internal class EntityMapping<T> : EntityMapping where T : class
    {
        public EntityMapping()
            : base(typeof(T))
        {
        }
    }




}
