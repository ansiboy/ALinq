using System;
using ALinq.Mapping;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlUserRow : SqlSimpleTypeExpression
    {
        // Fields
        private SqlUserQuery query;
        private MetaType rowType;

        // Methods
        internal SqlUserRow(MetaType rowType, IProviderType sqlType, SqlUserQuery query, Expression source)
            : base(SqlNodeType.UserRow, rowType.Type, sqlType, source)
        {
            this.Query = query;
            this.rowType = rowType;
        }

        // Properties
        internal SqlUserQuery Query
        {
            get
            {
                return this.query;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if ((value.Projection != null) && (value.Projection.ClrType != base.ClrType))
                {
                    throw Error.ArgumentWrongType("value", base.ClrType, value.Projection.ClrType);
                }
                this.query = value;
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