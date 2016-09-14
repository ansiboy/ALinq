using System;
using System.Diagnostics;

namespace ALinq.SqlClient
{
    internal class SqlSharedExpression : SqlExpression
    {
       
        // Fields
        private SqlExpression expr;

        // Methods
        internal SqlSharedExpression(SqlExpression expr)
            : base(SqlNodeType.SharedExpression, expr.ClrType, expr.SourceExpression)
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
                if (!base.ClrType.IsAssignableFrom(value.ClrType) && !value.ClrType.IsAssignableFrom(base.ClrType))
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