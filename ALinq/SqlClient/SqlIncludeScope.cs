using System.Diagnostics;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlIncludeScope : SqlNode
    {
          // Fields

        // Methods
        [DebuggerStepThrough]
        internal SqlIncludeScope(SqlNode child, Expression sourceExpression)
            : base(SqlNodeType.IncludeScope, sourceExpression)
        {
            this.Child = child;
        }

        // Properties
        internal SqlNode Child { get; set; }
    }
}