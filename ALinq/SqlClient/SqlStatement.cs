using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlStatement : SqlNode
    {
         // Methods
        internal SqlStatement(SqlNodeType nodeType, Expression sourceExpression)
            : base(nodeType, sourceExpression)
        {
        }

    }
}