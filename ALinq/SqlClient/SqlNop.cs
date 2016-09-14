using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlNop : SqlSimpleTypeExpression
    {
       
        // Methods
        internal SqlNop(Type clrType, IProviderType sqlType, Expression sourceExpression)
            : base(SqlNodeType.Nop, clrType, sqlType, sourceExpression)
        {
        }

    }
}