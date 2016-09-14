using System;
using System.Reflection;

namespace ALinq.Mapping
{
    internal sealed class UnmappedTable : MetaTable
    {
        private string tableName;
        private UnmappedType rowType;
        private MetaModel model;
        private Type type;

        public UnmappedTable(MetaModel model, Type type)
            : this(model, type, type.Name)
        {

        }
        public UnmappedTable(MetaModel model, Type type, string tableName)
        {
            this.model = model;
            this.type = type;
            this.tableName = tableName;
        }

        public override MetaModel Model
        {
            get { return this.model; }
        }

        public override MetaType RowType
        {
            get
            {
                if (rowType == null)
                    rowType = new UnmappedType(model, type);

                return rowType;
            }
        }

        public override string TableName
        {
            get { return this.tableName; }
        }

        public override MethodInfo DeleteMethod
        {
            get { return null; }
        }

        public override MethodInfo InsertMethod
        {
            get { return null; }
        }

        public override MethodInfo UpdateMethod
        {
            get { return null; }
        }
    }
}