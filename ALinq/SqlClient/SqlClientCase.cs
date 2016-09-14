using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlClientCase : SqlExpression
    {
        
        // Fields
        private SqlExpression expression;
        private List<SqlClientWhen> whens;

        // Methods
        internal SqlClientCase(Type clrType, SqlExpression expr, IEnumerable<SqlClientWhen> whens, Expression sourceExpression)
            : base(SqlNodeType.ClientCase, clrType, sourceExpression)
        {
            this.whens = new List<SqlClientWhen>();
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

        internal List<SqlClientWhen> Whens
        {
            get
            {
                return this.whens;
            }
        }

    }
}