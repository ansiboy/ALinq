using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlUserQuery : SqlNode
    {
        // Fields
        private SqlExpression projection;

        // Methods
        internal SqlUserQuery(SqlNodeType nt, SqlExpression projection, IEnumerable<SqlExpression> args, Expression source)
            : base(nt, source)
        {
            this.Projection = projection;
            this.Arguments = (args != null) ? new List<SqlExpression>(args) : new List<SqlExpression>();
            this.Columns = new List<SqlUserColumn>();
        }

        internal SqlUserQuery(string queryText, SqlExpression projection, IEnumerable<SqlExpression> args, Expression source)
            : base(SqlNodeType.UserQuery, source)
        {
            this.QueryText = queryText;
            this.Projection = projection;
            this.Arguments = (args != null) ? new List<SqlExpression>(args) : new List<SqlExpression>();
            this.Columns = new List<SqlUserColumn>();
        }

        internal SqlUserColumn Find(string name)
        {
            foreach (SqlUserColumn column in this.Columns)
            {
                if (column.Name == name)
                {
                    return column;
                }
            }
            return null;
        }

        // Properties
        internal List<SqlExpression> Arguments
        {
            [DebuggerStepThrough]
            get;
            private set;
        }

        internal List<SqlUserColumn> Columns
        {
            [DebuggerStepThrough]
            get;
            private set;
        }

        internal SqlExpression Projection
        {
            [DebuggerStepThrough]
            get
            {
                return this.projection;
            }
            set
            {
                if ((this.projection != null) && (this.projection.ClrType != value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.projection.ClrType, value.ClrType);
                }
                this.projection = value;
            }
        }

        internal string QueryText { get; private set; }
    }
}