using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlIn : SqlSimpleTypeExpression
    {
      
        // Fields
        private SqlExpression expression;
        private List<SqlExpression> values;

        // Methods
        internal SqlIn(Type clrType, IProviderType sqlType, SqlExpression expression, IEnumerable<SqlExpression> values, Expression sourceExpression)
            : base(SqlNodeType.In, clrType, sqlType, sourceExpression)
        {
            this.expression = expression;
            this.values = (values != null) ? new List<SqlExpression>(values) : new List<SqlExpression>(0);
        }

        // Properties
        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.expression = value;
            }
        }

        internal List<SqlExpression> Values
        {
            get
            {
                return this.values;
            }
        }

    }
}