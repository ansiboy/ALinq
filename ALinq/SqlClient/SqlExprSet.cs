using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlExprSet : SqlExpression
    {
         // Fields
        private List<SqlExpression> expressions;

        // Methods
        internal SqlExprSet(Type clrType, IEnumerable<SqlExpression> exprs, Expression sourceExpression)
            : base(SqlNodeType.ExprSet, clrType, sourceExpression)
        {
            this.expressions = new List<SqlExpression>(exprs);
        }

        internal SqlExpression GetFirstExpression()
        {
            SqlExpression expression = this.expressions[0];
            while (expression is SqlExprSet)
            {
                expression = ((SqlExprSet)expression).Expressions[0];
            }
            return expression;
        }

        // Properties
        internal List<SqlExpression> Expressions
        {
            get
            {
                return this.expressions;
            }
        }

        internal override IProviderType SqlType
        {
            get
            {
                return this.expressions[0].SqlType;
            }
        }

    }
}