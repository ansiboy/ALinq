using System;

namespace ALinq.SqlClient
{
    internal class SqlOrderExpression
    {
        // Fields
        private SqlExpression expression;
        private SqlOrderType orderType;

        // Methods
        internal SqlOrderExpression(SqlOrderType type, SqlExpression expr)
        {
            this.OrderType = type;
            this.Expression = expr;
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
                    throw Error.ArgumentNull("value");
                }
                if ((this.expression != null) && !this.expression.ClrType.IsAssignableFrom(value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.expression.ClrType, value.ClrType);
                }
                this.expression = value;
            }
        }

        internal SqlOrderType OrderType
        {
            get
            {
                return this.orderType;
            }
            set
            {
                this.orderType = value;
            }
        }
    }


}