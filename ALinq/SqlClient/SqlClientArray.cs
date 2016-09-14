using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlClientArray : SqlSimpleTypeExpression
    {
       
        // Fields
        private List<SqlExpression> expressions;

        // Methods
        internal SqlClientArray(Type clrType, IProviderType sqlType, SqlExpression[] exprs, Expression sourceExpression)
            : base(SqlNodeType.ClientArray, clrType, sqlType, sourceExpression)
        {
            this.expressions = new List<SqlExpression>();
            if (exprs != null)
            {
                this.Expressions.AddRange(exprs);
            }
        }

        // Properties
        internal List<SqlExpression> Expressions
        {
            get
            {
                return this.expressions;
            }
        }

    }
}