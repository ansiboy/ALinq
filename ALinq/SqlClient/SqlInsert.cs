using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlInsert : SqlStatement
    {
       
        // Fields
        private SqlExpression expression;
        private SqlTable table;

        // Methods
        internal SqlInsert(SqlTable table, SqlExpression expr, Expression sourceExpression)
            : base(SqlNodeType.Insert, sourceExpression)
        {
            this.Table = table;
            this.Expression = expr;
            this.Row = new SqlRow(sourceExpression);
        }

        // Properties
        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("null");
                }
                if (!this.table.RowType.Type.IsAssignableFrom(value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.table.RowType, value.ClrType);
                }
                this.expression = value;
            }
        }

        internal SqlColumn OutputKey { get; set; }

        internal bool OutputToLocal { get; set; }

        internal SqlRow Row { get; set; }

        internal SqlTable Table
        {
            get
            {
                return this.table;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("null");
                }
                this.table = value;
            }
        }

    }
}