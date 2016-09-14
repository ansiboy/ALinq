using System;

namespace ALinq.SqlClient
{
    internal class SqlSimpleExpression : SqlExpression
    {
       // Fields
        private SqlExpression expr;

        // Methods
        internal SqlSimpleExpression(SqlExpression expr)
            : base(SqlNodeType.SimpleExpression, expr.ClrType, expr.SourceExpression)
        {
            this.expr = expr;
        }

        // Properties
        internal SqlExpression Expression
        {
            get
            {
                return this.expr;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if (!base.ClrType.IsAssignableFrom(value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", base.ClrType, value.ClrType);
                }
                this.expr = value;
            }
        }

        internal override IProviderType SqlType
        {
            get
            {
                return this.expr.SqlType;
            }
        }

    }
}