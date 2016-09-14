using System.Linq;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal class DynamicMetaTable : MetaTable
    {
        private readonly MetaTable source;
        private readonly DynamicModel model;
        //private MetaDataMember itemMember;

        internal DynamicMetaTable(DynamicModel model, MetaTable source)
        {
            this.model = model;
            this.source = source;

            //itemMember = source.RowType.InheritanceRoot.PersistentDataMembers.FirstOrDefault(DynamicMappingSource.ItemMemberPredicate);
        }

        public override MethodInfo DeleteMethod
        {
            get { return source.DeleteMethod; }
        }

        public override MethodInfo InsertMethod
        {
            get { return source.InsertMethod; }
        }

        public override MetaModel Model
        {
            get { return this.model; }
        }

        public override MetaType RowType
        {
            get
            {
                var metaType = source.RowType;
                return model.GetMetaTypeBySource(metaType);
            }
        }

        public override string TableName
        {
            get { return source.TableName; }
        }

        public override MethodInfo UpdateMethod
        {
            get { return source.UpdateMethod; }
        }

        //public bool ContainsItemMember
        //{
        //    get { return this.itemMember != null; }
        //}

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (DynamicMetaTable)) 
                return false;
            return Equals((DynamicMetaTable) obj);
        }

        public bool Equals(DynamicMetaTable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.source, source);
        }

        public override int GetHashCode()
        {
            return (source != null ? source.GetHashCode() : 0);
        }
    }
}
