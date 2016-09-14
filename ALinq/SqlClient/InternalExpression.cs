using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal abstract class InternalExpression : Expression
    {
        // Methods
        internal InternalExpression(InternalExpressionType nt, Type type)
            : base((ExpressionType) nt, type)
        {
        }

        internal static KnownExpression Known(SqlExpression expr)
        {
            return new KnownExpression(expr, expr.ClrType);
        }

        internal static KnownExpression Known(SqlNode node, Type type)
        {
            return new KnownExpression(node, type);
        }
    }
}