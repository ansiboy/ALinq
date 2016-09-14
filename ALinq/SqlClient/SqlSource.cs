using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal abstract class SqlSource : SqlNode
    {
      
        // Methods
        internal SqlSource(SqlNodeType nt, Expression sourceExpression)
            : base(nt, sourceExpression)
        {
        }

    }
}