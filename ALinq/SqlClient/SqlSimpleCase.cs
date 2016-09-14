using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlSimpleCase : SqlExpression
    {
       
        // Fields
        private SqlExpression expression;
        private List<SqlWhen> whens;

        // Methods
        internal SqlSimpleCase(Type clrType, SqlExpression expr, IEnumerable<SqlWhen> whens, Expression sourceExpression)
            : base(SqlNodeType.SimpleCase, clrType, sourceExpression)
        {
            this.whens = new List<SqlWhen>();
            this.Expression = expr;
            if (whens == null)
            {
                throw Error.ArgumentNull("whens");
            }
            this.whens.AddRange(whens);
            if (this.whens.Count == 0)
            {
                throw Error.ArgumentOutOfRange("whens");
            }
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
                if ((this.expression != null) && (this.expression.ClrType != value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.expression.ClrType, value.ClrType);
                }
                this.expression = value;
            }
        }

        internal override IProviderType SqlType
        {
            get
            {
                return this.whens[0].Value.SqlType;
            }
        }

        internal List<SqlWhen> Whens
        {
            get
            {
                return this.whens;
            }
        }

    }
}