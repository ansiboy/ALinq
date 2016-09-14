using System;
using System.Diagnostics;

namespace ALinq.SqlClient
{
    internal class SqlSharedExpressionRef : SqlExpression
    {
        // Fields
        private SqlSharedExpression expr;

        // Methods
        internal SqlSharedExpressionRef(SqlSharedExpression expr)
            : base(SqlNodeType.SharedExpressionRef, expr.ClrType, expr.SourceExpression)
        {
            this.expr = expr;
        }

        // Properties
        internal SqlSharedExpression SharedExpression
        {
            get
            {
                return this.expr;
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