using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlTableValuedFunctionCall : SqlFunctionCall
    {
       
        // Fields
        private List<SqlColumn> columns;
        private MetaType rowType;

        // Methods
        internal SqlTableValuedFunctionCall(MetaType rowType, Type clrType, IProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source)
            : base(SqlNodeType.TableValuedFunctionCall, clrType, sqlType, name, args, source)
        {
            this.rowType = rowType;
            this.columns = new List<SqlColumn>();
        }

        internal SqlColumn Find(string name)
        {
            foreach (SqlColumn column in this.Columns)
            {
                if (column.Name == name)
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

        internal MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

    }
}