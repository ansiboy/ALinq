using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlTable : SqlNode
    {
       
        // Fields
        private List<SqlColumn> columns;
        private MetaType rowType;
        private IProviderType sqlRowType;
        private Mapping.MetaTable table;

        // Methods
        internal SqlTable(Mapping.MetaTable table, MetaType rowType, IProviderType sqlRowType, Expression sourceExpression)
            : base(SqlNodeType.Table, sourceExpression)
        {
            this.table = table;
            this.rowType = rowType;
            this.sqlRowType = sqlRowType;
            this.columns = new List<SqlColumn>();
        }

        internal SqlColumn Find(string columnName)
        {
            foreach (SqlColumn column in this.Columns)
            {
                if (column.Name == columnName)
                {
                    return column;
                }
            }
            return null;
        }

        // Properties
        internal List<SqlColumn> Columns
        {
            get
            {
                return this.columns;
            }
        }

        internal MetaTable MetaTable
        {
            get
            {
                return this.table;
            }
        }

        internal string Name
        {
            get
            {
                return this.table.TableName;
            }
        }

        internal MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

        internal IProviderType SqlRowType
        {
            get
            {
                return this.sqlRowType;
            }
        }

    }
}