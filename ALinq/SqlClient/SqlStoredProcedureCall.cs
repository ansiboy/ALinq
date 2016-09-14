using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlStoredProcedureCall : SqlUserQuery
    {
          // Fields
        private readonly MetaFunction function;

        // Methods
        internal SqlStoredProcedureCall(MetaFunction function, SqlExpression projection, IEnumerable<SqlExpression> args, 
                                        Expression source)
            : base(SqlNodeType.StoredProcedureCall, projection, args, source)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }
            this.function = function;
        }

        // Properties
        internal MetaFunction Function
        {
            get
            {
                return function;
            }
        }

    }
}