using System;
using System.Diagnostics;

namespace ALinq.SqlClient
{
    internal class SqlDoNotVisitExpression : SqlExpression
    {
     
        // Fields
        private readonly SqlExpression expression;

        // Methods
        internal SqlDoNotVisitExpression(SqlExpression expr)
            : base(SqlNodeType.DoNotVisit, expr.ClrType, expr.SourceExpression)
        {
            if (expr == null)
            {
                throw Error.ArgumentNull("expr");
            }
            this.expression = expr;
        }

        // Properties
        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
        }

        internal override IProviderType SqlType
        {
            get
            {
                return this.expression.SqlType;
            }
        }

    }
}