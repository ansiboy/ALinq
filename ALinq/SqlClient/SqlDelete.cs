using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlDelete : SqlStatement
    {
            // Fields
        private SqlSelect select;

        // Methods
        internal SqlDelete(SqlSelect select, Expression sourceExpression)
            : base(SqlNodeType.Delete, sourceExpression)
        {
            this.Select = select;
        }

        // Properties
        internal SqlSelect Select
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return select;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.select = value;
            }
        }

    }
}